using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Palmmedia.ReportGenerator.Core;
using Palmmedia.ReportGenerator.Core.Logging;

namespace FineCodeCoverage.ReportGeneratorTool
{
    // Generates a ReportGenerator report for the FCC extension.  This must run out-of-process - inside
    // devenv the Microsoft.Extensions.* assembly family splits between the copies VS has loaded and the
    // copies FCC ships, which breaks Generator.GetConfiguration with FileNotFound/MissingMethod exceptions
    // however resolution is attempted (VS's AppDomainManager pollutes child AppDomains too).
    //
    // Usage: FineCodeCoverage.ReportGeneratorTool.exe <response file>
    //   response file line 0 - report output directory
    //   response file line 1 - verbosity (ReportGenerator VerbosityLevel name)
    //   response file line 2 - report types separated by ;
    //   remaining lines      - coverage files
    // Log messages are written to stdout as "<int verbosity level>|<message>".
    internal static class Program
    {
        private static int Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveFromBaseDirectory;
            try
            {
                if (args.Length != 1 || !File.Exists(args[0]))
                {
                    Console.Error.WriteLine("Expected a single argument - the path to the report generation response file.");
                    return 2;
                }

                return GenerateReport(File.ReadAllLines(args[0])) ? 0 : 1;
            }
            catch (Exception exc)
            {
                Console.Error.WriteLine(exc.ToString());
                return 1;
            }
        }

        private static bool GenerateReport(string[] responseLines)
        {
            string reportDirectory = responseLines[0];
            string verbosity = responseLines[1];
            string[] reportTypes = responseLines[2].Split(';');
            List<string> coverageFiles = responseLines.Skip(3).Where(line => !string.IsNullOrWhiteSpace(line)).ToList();

            LoggerFactory.Configure((level, message) => Console.WriteLine($"{(int)level}|{message}"));
            LoggerFactory.VerbosityLevel = (VerbosityLevel)Enum.Parse(typeof(VerbosityLevel), verbosity);

            IEnumerable<string> empty = Enumerable.Empty<string>();
            var config = new ReportConfiguration(
                new ReadOnlyCollection<string>(coverageFiles),
                reportDirectory,
                empty,
                null,
                new ReadOnlyCollection<string>(reportTypes.ToList()),
                empty,
                empty,
                empty,
                empty,
                verbosity,
                string.Empty);

            return new Generator().GenerateReport(config);
        }

        private static Assembly ResolveFromBaseDirectory(object sender, ResolveEventArgs args)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, new AssemblyName(args.Name).Name + ".dll");
            return File.Exists(path) ? Assembly.LoadFrom(path) : null;
        }
    }
}
