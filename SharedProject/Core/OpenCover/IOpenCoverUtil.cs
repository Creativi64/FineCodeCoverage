using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Engine.Model;

namespace FineCodeCoverage.Engine.OpenCover
{
    internal interface IOpenCoverUtil
    {
        Task RunOpenCoverAsync(ICoverageProject project, CancellationToken cancellationToken);
        void Initialize(string appDataFolder, CancellationToken cancellationToken);
    }
}
