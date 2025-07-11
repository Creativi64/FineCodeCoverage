using System;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.VSAbstractions.Threading;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Core.Utilities.VsThreading
{
    internal sealed class VsJoinableTaskFactory : IJoinableTaskFactory
    {
        public T Run<T>(Func<Task<T>> asyncMethod)
#pragma warning disable VSTHRD102 // Implement internal logic asynchronously
            => ThreadHelper.JoinableTaskFactory.Run(asyncMethod);
#pragma warning restore VSTHRD102 // Implement internal logic asynchronously

        public void Run(Func<Task> asyncMethod)
#pragma warning disable VSTHRD102 // Implement internal logic asynchronously
            => ThreadHelper.JoinableTaskFactory.Run(asyncMethod);
#pragma warning restore VSTHRD102 // Implement internal logic asynchronously

        public async Task SwitchToMainThreadAsync(CancellationToken cancellationToken = default)
            => await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
    }
}
