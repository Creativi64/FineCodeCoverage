using FineCodeCoverage.Output;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;


namespace FineCodeCoverage.Engine.ReportGenerator
{
    interface IDynamicReportResult : IReportResult { }
    internal class DynamicReportResult : IDynamicReportResult
    {
        public IReadOnlyList<IAssembly> Assemblies { get; set; }
        public IReadOnlyList<MetricType> MetricTypes { get; set; }
    }


    [Export(typeof(IReportGeneratorUtil))]
    internal partial class ReportGeneratorUtil : IReportGeneratorUtil
    {
        private readonly IFCCReportGenerator reportGenerator;
        private readonly ILogger logger;
        private List<string> logs;

        [ImportingConstructor]
        public ReportGeneratorUtil(
            IFCCReportGenerator reportGenerator,
            ILogger logger
        )
        {
            this.reportGenerator = reportGenerator;
            this.logger = logger;
            this.reportGenerator.SetLogger(VerbosityLevel.Info, (_, message) => logs.Add(message));
        }

        private IDynamicReportResult CreateDynamicReport(IReportResult reportResult)
            => new DynamicReportResult { Assemblies = reportResult.Assemblies, MetricTypes = reportResult.MetricTypes };

        public async Task<ReportGeneratorResult> GenerateAsync(
            IEnumerable<string> coverOutputFiles,
            string reportOutputFolder,
            CancellationToken cancellationToken)
        {
            logs = new List<string>();
            logs.Add("Report Generator - Output");
            var reportResult = this.reportGenerator.Generate(coverOutputFiles, reportOutputFolder, new List<string> { "Cobertura", "HtmlSummary" });
            await logger.LogAsync(logs);
            return new ReportGeneratorResult
            {
                ReportResult = CreateDynamicReport(reportResult),
                UnifiedXmlFile = Path.Combine(reportOutputFolder, "Cobertura.xml"),
            };
        }
    }
}
