using System.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverage.Collection.CoverletOpenCover.Process
{
    internal interface IProcessUtil
    {
        Task<ExecuteResponse> ExecuteAsync(ExecuteRequest request, CancellationToken cancellationToken);
    }
}
