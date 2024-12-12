using System.Collections.Generic;
using System.Threading;

namespace FineCodeCoverage.Engine.ReportGenerator
{
    internal class ReportGeneratorResult
    {
        public IReportResult ReportResult { get; set; }
        public string UnifiedXmlFile { get; set; }
    }

    internal interface IReportGeneratorUtil
    {
        ReportGeneratorResult Generate(
            IEnumerable<string> coverOutputFiles,
            string reportOutputFolder,
            CancellationToken cancellationToken
        );
    }
}
