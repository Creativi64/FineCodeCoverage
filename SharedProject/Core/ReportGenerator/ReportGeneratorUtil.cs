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
        private readonly IFCCReportGenerator reportGenerator;
        private readonly ILogger logger;
        private List<string> logs;
        private DynamicReportResult dynamicReportResult;

        [ImportingConstructor]
        public ReportGeneratorUtil(
            IFCCReportGenerator reportGenerator,
            ILogger logger,
            IFileRenameListener fileRenameListener
        )
        {
            this.reportGenerator = reportGenerator;
            this.logger = logger;
            this.reportGenerator.SetLogger(VerbosityLevel.Info, (_, message) => this.logs.Add(message));
            fileRenameListener.FileRenamedEvent += this.FileRenameListener_FileRenamedEvent;
        }

        private void FileRenameListener_FileRenamedEvent(IReadOnlyList<FileRename> fileRenames)
            => this.dynamicReportResult?.FileRenamed(fileRenames);

        public async Task<ReportGeneratorResult> GenerateAsync(
            IEnumerable<string> coverOutputFiles,
            string reportOutputFolder,
            CancellationToken cancellationToken)
        {
            this.logs = new List<string>
            {
                "Report Generator - Output"
            };
            IReportResult reportResult = this.reportGenerator.Generate(coverOutputFiles, reportOutputFolder, new List<string> { "Cobertura", "HtmlSummary" });
            await this.logger.LogAsync(this.logs);
            this.dynamicReportResult = DynamicReportResult.FromReportResult(reportResult);
            return new ReportGeneratorResult
            {
                ReportResult = this.dynamicReportResult,
                UnifiedXmlFile = Path.Combine(reportOutputFolder, "Cobertura.xml"),
            };
        }
    }
}
