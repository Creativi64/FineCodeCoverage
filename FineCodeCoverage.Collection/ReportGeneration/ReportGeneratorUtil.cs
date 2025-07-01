using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Output;

namespace FineCodeCoverage.Collection.ReportGeneration
{
    [Export(typeof(IReportGeneratorUtil))]
    internal sealed class ReportGeneratorUtil : IReportGeneratorUtil
    {
        private readonly IFCCReportGenerator _reportGenerator;
        private readonly ILogger _logger;
        private List<string> _logs;
        private DynamicReportResult _dynamicReportResult;

        [ImportingConstructor]
        public ReportGeneratorUtil(
            IFCCReportGenerator reportGenerator,
            ILogger logger,
            IFileRenameListener fileRenameListener)
        {
            _reportGenerator = reportGenerator;
            _logger = logger;
            _reportGenerator.SetLogger(VerbosityLevel.Info, (_, message) => _logs.Add(message));
            fileRenameListener.FileRenamedEvent += FileRenameListener_FileRenamedEvent;
        }

        private void FileRenameListener_FileRenamedEvent(IReadOnlyList<FileRename> fileRenames)
            => _dynamicReportResult?.FileRenamed(fileRenames);

        public async Task<ReportGeneratorResult> GenerateAsync(
            IEnumerable<string> coverOutputFiles,
            string reportOutputFolder,
            CancellationToken cancellationToken)
        {
            _logs = new List<string>
            {
                "Report Generator - Output",
            };
            IReportResult reportResult = _reportGenerator.Generate(coverOutputFiles, reportOutputFolder, new List<string> { "Cobertura", "HtmlSummary" });
            await _logger.LogAsync(_logs);
            _dynamicReportResult = DynamicReportResult.FromReportResult(reportResult);
            return new ReportGeneratorResult
            {
                ReportResult = _dynamicReportResult,
                UnifiedXmlFile = Path.Combine(reportOutputFolder, "Cobertura.xml"),
            };
        }
    }
}
