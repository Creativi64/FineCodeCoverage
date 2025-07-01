using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverage.Engine.ReportGenerator
{
    public interface IReportGeneratorUtil
    {
        Task<ReportGeneratorResult> GenerateAsync(
            IEnumerable<string> coverOutputFiles,
            string reportOutputFolder,
            CancellationToken cancellationToken);
    }
}
