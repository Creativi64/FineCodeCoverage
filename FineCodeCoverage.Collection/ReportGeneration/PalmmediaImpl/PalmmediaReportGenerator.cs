using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Parser;
using Palmmedia.ReportGenerator.Core.Parser.Filtering;
using PalmmediaVerbosityLevel = Palmmedia.ReportGenerator.Core.Logging.VerbosityLevel;

namespace FineCodeCoverage.Collection.ReportGeneration.PalmmediaImpl
{
    [Export(typeof(IFCCReportGenerator))]
    internal sealed class PalmmediaReportGenerator
        : IFCCReportGenerator
    {
        private readonly Regex _fileDoesNotExistAnymoreRegex = new Regex(@"File '.*' does not exist \(any more\)\.", RegexOptions.Compiled);
        private readonly IHtmlFilesToFolder _htmlFilesToFolder;
        private VerbosityLevel _verbosityLevel;
        private Action<VerbosityLevel, string> _logger;

        [ImportingConstructor]
        public PalmmediaReportGenerator(
            IHtmlFilesToFolder htmlFilesToFolder) => _htmlFilesToFolder = htmlFilesToFolder;

        public IReportResult Generate(IEnumerable<string> coverageFiles, string reportDirectory, IEnumerable<string> reportTypes)
        {
            var defaultFilter = new DefaultFilter(Array.Empty<string>());
            var parser = new CoverageReportParser(1, 1, Enumerable.Empty<string>(), defaultFilter, defaultFilter, defaultFilter);
            var collection = new ReadOnlyCollection<string>(coverageFiles.ToList());

            // ReportGenerator.Core's transitive dependencies ship with a different assembly revision than the
            // version it was compiled against, which fails strong-name binding in the VS (devenv) host where no
            // binding redirect applies.  Resolve them by simple name for the duration of the parse.
            AppDomain.CurrentDomain.AssemblyResolve += ResolveReportGeneratorDependency;
            ParserResult parserResult;
            try
            {
                parserResult = parser.ParseFiles(collection);
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= ResolveReportGeneratorDependency;
            }

            // Generator.GenerateReport pulls in Microsoft.Extensions.Configuration.* which conflicts with the
            // copies VS itself has loaded (devenv's AppDomainManager pollutes child AppDomains too) - run it in
            // a separate process instead (FineCodeCoverage.ReportGeneratorTool.exe, shipped in the extension dir).
            GenerateReportOutOfProcess(coverageFiles, reportDirectory, reportTypes);
            _htmlFilesToFolder.Collate(reportDirectory);
            return new PalmmediaReportResult(parserResult);
        }

        private void GenerateReportOutOfProcess(IEnumerable<string> coverageFiles, string reportDirectory, IEnumerable<string> reportTypes)
        {
            string extensionDirectory = Path.GetDirectoryName(typeof(PalmmediaReportGenerator).Assembly.Location);
            string exePath = Path.Combine(extensionDirectory, "FineCodeCoverage.ReportGeneratorTool.exe");
            string responseFile = Path.Combine(Path.GetTempPath(), $"fcc-report-{Guid.NewGuid():N}.txt");
            var responseLines = new List<string> { reportDirectory, _verbosityLevel.ToString(), string.Join(";", reportTypes) };
            responseLines.AddRange(coverageFiles);
            File.WriteAllLines(responseFile, responseLines);
            try
            {
                using (var process = new Process())
                {
                    process.StartInfo = new ProcessStartInfo
                    {
                        FileName = exePath,
                        Arguments = $"\"{responseFile}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                    };
                    var allOutput = new StringBuilder();
                    process.OutputDataReceived += (_, e) =>
                    {
                        if (e.Data != null)
                        {
                            _ = allOutput.AppendLine(e.Data);
                        }

                        LogToolOutput(e.Data);
                    };
                    process.ErrorDataReceived += (_, e) =>
                    {
                        if (e.Data != null)
                        {
                            _ = allOutput.AppendLine(e.Data);
                        }
                    };
                    _ = process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();
                    if (process.ExitCode != 0)
                    {
                        throw new InvalidOperationException($"Report generation failed with exit code {process.ExitCode}.{Environment.NewLine}{allOutput}");
                    }
                }
            }
            finally
            {
                try
                {
                    File.Delete(responseFile);
                }
                catch (IOException)
                {
                }
            }
        }

        private void LogToolOutput(string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                return;
            }

            var palmmediaVerbosityLevel = PalmmediaVerbosityLevel.Info;
            string message = line;
            int separatorIndex = line.IndexOf('|');
            if (separatorIndex > 0 && int.TryParse(line.Substring(0, separatorIndex), out int verbosityLevel))
            {
                palmmediaVerbosityLevel = (PalmmediaVerbosityLevel)verbosityLevel;
                message = line.Substring(separatorIndex + 1);
            }

            if (ShouldLog(palmmediaVerbosityLevel, message))
            {
                _logger?.Invoke((VerbosityLevel)palmmediaVerbosityLevel, message);
            }
        }

        private Assembly ResolveReportGeneratorDependency(object sender, ResolveEventArgs args)
        {
            string simpleName = new AssemblyName(args.Name).Name;

            // Detach while we resolve so a nested resolve does not recurse into this handler.
            AppDomain.CurrentDomain.AssemblyResolve -= ResolveReportGeneratorDependency;
            try
            {
                // Load by simple name so the whole Microsoft.Extensions.* family unifies in the default
                // load context (via the extension's binding path) regardless of the exact revision the
                // requesting assembly was compiled against.  LoadFrom would create a separate identity and
                // cause MissingMethodException across the dependency diamond.
                try
                {
                    Assembly byName = Assembly.Load(simpleName);
                    if (byName != null)
                    {
                        return byName;
                    }
                }
                catch
                {
                    // fall through to path-based load
                }

                string directory = Path.GetDirectoryName(typeof(PalmmediaReportGenerator).Assembly.Location);
                string path = Path.Combine(directory, simpleName + ".dll");
                return File.Exists(path) ? Assembly.LoadFrom(path) : null;
            }
            catch
            {
                return null;
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve += ResolveReportGeneratorDependency;
            }
        }

        public void SetLogger(VerbosityLevel verbosityLevel, Action<VerbosityLevel, string> logger)
        {
            _verbosityLevel = verbosityLevel;
            _logger = logger;
            LoggerFactory.Configure((palmmediaVerbosityLevel, message) =>
            {
                bool shouldLog = ShouldLog(palmmediaVerbosityLevel, message);
                if (!shouldLog)
                {
                    return;
                }

                logger((VerbosityLevel)palmmediaVerbosityLevel, message);
            });
            LoggerFactory.VerbosityLevel = (PalmmediaVerbosityLevel)verbosityLevel;
        }

        private bool ShouldLog(PalmmediaVerbosityLevel palmmediaVerbosityLevel, string message)
        {
            bool shouldLog = true;
            if (palmmediaVerbosityLevel != PalmmediaVerbosityLevel.Error)
            {
                Match matched = _fileDoesNotExistAnymoreRegex.Match(message);
                shouldLog = !matched.Success;
            }

            return shouldLog;
        }
    }
}
