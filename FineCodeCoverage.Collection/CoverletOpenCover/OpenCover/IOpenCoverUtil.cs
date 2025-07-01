using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Collection.CoverageProjectManagement;

namespace FineCodeCoverage.Collection.CoverletOpenCover.OpenCover
{
    internal interface IOpenCoverUtil
    {
        Task RunOpenCoverAsync(ICoverageProject project, CancellationToken cancellationToken);

        void Initialize(string appDataFolder, CancellationToken cancellationToken);
    }
}
