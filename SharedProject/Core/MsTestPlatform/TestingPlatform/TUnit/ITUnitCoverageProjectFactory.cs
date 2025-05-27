using System.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    internal interface ITUnitCoverageProjectFactory
    {
        Task<ITUnitCoverageProject> CreateTUnitCoverageProjectAsync(
            ITUnitProject tUnitProject, CancellationToken cancellationToken);
    }
}
