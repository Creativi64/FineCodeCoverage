using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;


namespace FineCodeCoverage.Engine.ReportGenerator
{
    [Export(typeof(IReportGeneratorUtil))]
    internal partial class ReportGeneratorUtil : IReportGeneratorUtil
    {
        private readonly IFCCReportGenerator reportGenerator;

        [ImportingConstructor]
        public ReportGeneratorUtil(
            IFCCReportGenerator reportGenerator,
            ILogger logger
        )
        {
            this.reportGenerator = reportGenerator;
            this.reportGenerator.SetLogger(VerbosityLevel.Info, (level, message) =>
            {
                logger.Log(message);
            });
        }

        public ReportGeneratorResult Generate(
            IEnumerable<string> coverOutputFiles, 
            string reportOutputFolder, 
            CancellationToken cancellationToken)
        {
            var summaryResult = this.reportGenerator.Generate(coverOutputFiles, reportOutputFolder, new List<string> { "Cobertura", "HtmlSummary" });


            return new ReportGeneratorResult
            {
                ReportResult = summaryResult,
                UnifiedXmlFile = Path.Combine(reportOutputFolder, "Cobertura.xml"),
            };
        }
    }
}
