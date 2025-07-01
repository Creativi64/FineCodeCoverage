using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Collection.CoverageProjectManagement;

namespace FineCodeCoverage.Engine
{
    public interface ICoverageUtilManager
    {
        Task RunCoverageAsync(ICoverageProject project, CancellationToken cancellationToken);
    }
}
