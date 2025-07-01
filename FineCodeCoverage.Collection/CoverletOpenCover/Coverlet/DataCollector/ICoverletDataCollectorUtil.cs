using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Collection.CoverageProjectManagement;

namespace FineCodeCoverage.Collection.CoverletOpenCover.Coverlet.DataCollector
{
    internal interface ICoverletDataCollectorUtil
    {
        Task<bool> CanUseDataCollectorAsync(ICoverageProject coverageProject);

        Task RunAsync(CancellationToken cancellationToken);

        void Initialize(string appDataFolder, CancellationToken cancellationToken);
    }
}
