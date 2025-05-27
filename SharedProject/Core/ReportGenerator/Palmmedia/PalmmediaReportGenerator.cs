using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using Palmmedia.ReportGenerator.Core;
using Palmmedia.ReportGenerator.Core.Logging;
using Palmmedia.ReportGenerator.Core.Parser;
using Palmmedia.ReportGenerator.Core.Parser.Filtering;
using PalmmediaVerbosityLevel = Palmmedia.ReportGenerator.Core.Logging.VerbosityLevel;

namespace FineCodeCoverage.Engine.ReportGenerator
{
    [Export(typeof(IFCCReportGenerator))]
    internal class PalmmediaReportGenerator
        : IFCCReportGenerator
    {
        private readonly Regex fileDoesNotExistAnymoreRegex = new Regex(@"File '.*' does not exist \(any more\)\.", RegexOptions.Compiled);
        private readonly IHtmlFilesToFolder htmlFilesToFolder;

        [ImportingConstructor]
        public PalmmediaReportGenerator(
            IHtmlFilesToFolder htmlFilesToFolder
            )
        {
            this.htmlFilesToFolder = htmlFilesToFolder;
        }

        public IReportResult Generate(IEnumerable<string> coverageFiles, string reportDirectory, IEnumerable<string> reportTypes)
        {
            var empty = Enumerable.Empty<string>();
            var defaultFilter = new DefaultFilter(new string[] { });
            var config = new ReportConfiguration(
                new ReadOnlyCollection<string>(coverageFiles.ToList()),
                reportDirectory,
                empty,
                null,
                new ReadOnlyCollection<string>(reportTypes.ToList()),
                empty,
                empty,
                empty,
                empty,
                verbosityLevel.ToString(),
                ""
                );

            var parser = new CoverageReportParser(1, 1, Enumerable.Empty<string>(), defaultFilter, defaultFilter, defaultFilter);
            ReadOnlyCollection<string> collection = new ReadOnlyCollection<string>(coverageFiles.ToList());
            var parserResult = parser.ParseFiles(collection);
            new Generator().GenerateReport(config, parserResult);
            htmlFilesToFolder.Collate(reportDirectory);
            return new PalmmediaReportResult(parserResult);
        }

        private VerbosityLevel verbosityLevel;
        public void SetLogger(VerbosityLevel verbosityLevel, Action<VerbosityLevel, string> logger)
        {
            this.verbosityLevel = verbosityLevel;
            LoggerFactory.Configure((palmmediaVerbosityLevel, message) =>
            {
                var shouldLog = true;
                if (palmmediaVerbosityLevel != PalmmediaVerbosityLevel.Error)
                {
                    Match matched = this.fileDoesNotExistAnymoreRegex.Match(message);
                    shouldLog = !matched.Success;

                }
                if (shouldLog)
                {
                    logger((VerbosityLevel)palmmediaVerbosityLevel, message);
                }
            });
            LoggerFactory.VerbosityLevel = (PalmmediaVerbosityLevel)verbosityLevel;
        }
    }

}
