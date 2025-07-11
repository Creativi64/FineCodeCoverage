using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.VSAbstractions.Threading;
using Microsoft.VisualStudio.Threading;

namespace FineCodeCoverage.Core.Utilities.VsThreading
{
    [Export(typeof(IThreadHelper))]
    internal sealed class VsThreadHelper : IThreadHelper
    {
        public IJoinableTaskFactory JoinableTaskFactory { get; } = new VsJoinableTaskFactory();

        public async Task AwaitTaskSchedulerDefaultAsync() => await TaskScheduler.Default;

        public Task<int> WaitForForProcessExitAsync(Process process, CancellationToken cancellationToken = default) => process.WaitForExitAsync(cancellationToken);
    }
}
