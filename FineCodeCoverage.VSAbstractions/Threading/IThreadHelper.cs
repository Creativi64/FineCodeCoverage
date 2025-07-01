using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverage.Core.Utilities.VsThreading
{
    public interface IThreadHelper
    {
        IJoinableTaskFactory JoinableTaskFactory { get; }
        Task AwaitTaskSchedulerDefaultAsync();
        Task<int> WaitForForProcessExitAsync(Process process, CancellationToken cancellationToken = default(CancellationToken));
    }
}
