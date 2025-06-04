using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Output;

namespace FineCodeCoverage.Engine.ReportGenerator
{
    [Export(typeof(IReportGeneratorUtil))]
    internal class ReportGeneratorUtil : IReportGeneratorUtil
    {
        private readonly IFCCReportGenerator _reportGenerator;
        private readonly ILogger _logger;
        private List<string> _logs;
        private DynamicReportResult _dynamicReportResult;

        [ImportingConstructor]
        public ReportGeneratorUtil(
            IFCCReportGenerator reportGenerator,
            ILogger logger,
            IFileRenameListener fileRenameListener
        )
        {
            this._reportGenerator = reportGenerator;
            this._logger = logger;
            this._reportGenerator.SetLogger(VerbosityLevel.Info, (_, message) => this._logs.Add(message));
            fileRenameListener.FileRenamedEvent += this.FileRenameListener_FileRenamedEvent;
        }

        private void FileRenameListener_FileRenamedEvent(IReadOnlyList<FileRename> fileRenames)
            => this._dynamicReportResult?.FileRenamed(fileRenames);

        public async Task<ReportGeneratorResult> GenerateAsync(
            IEnumerable<string> coverOutputFiles,
            string reportOutputFolder,
            CancellationToken cancellationToken)
        {
            this._logs = new List<string>
            {
                "Report Generator - Output"
            };
            IReportResult reportResult = this._reportGenerator.Generate(coverOutputFiles, reportOutputFolder, new List<string> { "Cobertura", "HtmlSummary" });
            await this._logger.LogAsync(this._logs);
            this._dynamicReportResult = DynamicReportResult.FromReportResult(reportResult);
            return new ReportGeneratorResult
            {
                ReportResult = this._dynamicReportResult,
                UnifiedXmlFile = Path.Combine(reportOutputFolder, "Cobertura.xml"),
            };
        }
    }
}