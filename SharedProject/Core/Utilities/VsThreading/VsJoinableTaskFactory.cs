using System;
using Microsoft.VisualStudio.Shell;
using System.Threading.Tasks;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Core.Utilities.VsThreading
{
    internal class VsJoinableTaskFactory : IJoinableTaskFactory
    {
        public T Run<T>(Func<Task<T>> asyncMethod)
        {
#pragma warning disable VSTHRD102 // Implement internal logic asynchronously
            return ThreadHelper.JoinableTaskFactory.Run(asyncMethod);
#pragma warning restore VSTHRD102 // Implement internal logic asynchronously
        }

        public void Run(Func<Task> asyncMethod)
        {
#pragma warning disable VSTHRD102 // Implement internal logic asynchronously
            ThreadHelper.JoinableTaskFactory.Run(asyncMethod);
#pragma warning restore VSTHRD102 // Implement internal logic asynchronously
        }

        public async Task SwitchToMainThreadAsync(CancellationToken cancellationToken = default)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
        }
    }

}
