using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Collection.CoverageProjectManagement;

namespace FineCodeCoverage.Collection.CoverletOpenCover
{
    public interface ICoverageUtilManager
    {
        Task RunCoverageAsync(ICoverageProject project, CancellationToken cancellationToken);
    }
}
