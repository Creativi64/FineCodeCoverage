using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Engine.Model;

namespace FineCodeCoverage.Engine.Coverlet
{
    internal interface ICoverletDataCollectorUtil
    {
        Task<bool> CanUseDataCollectorAsync(ICoverageProject coverageProject);

        Task RunAsync(CancellationToken cancellationToken);

        void Initialize(string appDataFolder, CancellationToken cancellationToken);
    }
}
