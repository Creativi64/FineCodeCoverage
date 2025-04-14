using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverage.Engine.ReportGenerator
{
    internal class ReportGeneratorResult
    {
        public IDynamicReportResult ReportResult { get; set; }
        public string UnifiedXmlFile { get; set; }
    }

    internal interface IReportGeneratorUtil
    {
        Task<ReportGeneratorResult> GenerateAsync(
            IEnumerable<string> coverOutputFiles,
            string reportOutputFolder,
            CancellationToken cancellationToken
        );
    }
}
