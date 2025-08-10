using System.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverage.Collection.TestingPlatform.TUnit
{
    internal interface ITUnitCoverageProjectFactory
    {
        Task<ITUnitCoverageProject> CreateTUnitCoverageProjectAsync(
            ITUnitProject tUnitProject, CancellationToken cancellationToken);
    }
}
