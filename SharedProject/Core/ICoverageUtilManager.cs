using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Engine.Model;

namespace FineCodeCoverage.Engine
{
    internal interface ICoverageUtilManager
    {
        Task RunCoverageAsync(ICoverageProject project, CancellationToken cancellationToken);
    }
}
