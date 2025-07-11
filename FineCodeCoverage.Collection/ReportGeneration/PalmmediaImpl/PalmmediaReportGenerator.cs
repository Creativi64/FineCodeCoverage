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

namespace FineCodeCoverage.Collection.ReportGeneration.PalmmediaImpl
{
    [Export(typeof(IFCCReportGenerator))]
    internal sealed class PalmmediaReportGenerator
        : IFCCReportGenerator
    {
        private readonly Regex _fileDoesNotExistAnymoreRegex = new Regex(@"File '.*' does not exist \(any more\)\.", RegexOptions.Compiled);
        private readonly IHtmlFilesToFolder _htmlFilesToFolder;
        private VerbosityLevel _verbosityLevel;

        [ImportingConstructor]
        public PalmmediaReportGenerator(
            IHtmlFilesToFolder htmlFilesToFolder) => _htmlFilesToFolder = htmlFilesToFolder;

        public IReportResult Generate(IEnumerable<string> coverageFiles, string reportDirectory, IEnumerable<string> reportTypes)
        {
            IEnumerable<string> empty = Enumerable.Empty<string>();
            var defaultFilter = new DefaultFilter(Array.Empty<string>());
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
                _verbosityLevel.ToString(),
                string.Empty);

            var parser = new CoverageReportParser(1, 1, Enumerable.Empty<string>(), defaultFilter, defaultFilter, defaultFilter);
            var collection = new ReadOnlyCollection<string>(coverageFiles.ToList());
            ParserResult parserResult = parser.ParseFiles(collection);
            new Generator().GenerateReport(config, parserResult);
            _htmlFilesToFolder.Collate(reportDirectory);
            return new PalmmediaReportResult(parserResult);
        }

        public void SetLogger(VerbosityLevel verbosityLevel, Action<VerbosityLevel, string> logger)
        {
            _verbosityLevel = verbosityLevel;
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
