using System.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverage.Collection.TestingPlatform.TUnit
{
    internal interface ITUnitSettingsProvider
    {
        Task<TUnitSettings> ProvideAsync(ITUnitCoverageProject tUnitCoverageProject, CancellationToken cancellationToken);
    }
}
