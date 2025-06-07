using System.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverage.Core.Utilities
{
    internal interface IProcessUtil
    {
        Task<ExecuteResponse> ExecuteAsync(ExecuteRequest request, CancellationToken cancellationToken);
    }
}
