using FineCodeCoverage.Core.Utilities.VsThreading;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverageTests.TestHelpers
{
    internal class TestThreadHelper : IThreadHelper
    {
        public IJoinableTaskFactory JoinableTaskFactory { get; } = new TestJoinableTaskFactory();

        public Task AwaitTaskSchedulerDefaultAsync()
        {
            return Task.CompletedTask;
        }

        public Task<int> WaitForForProcessExitAsync(Process process, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    internal class TestJoinableTaskFactory : IJoinableTaskFactory
    {
        public void Run(Func<Task> asyncMethod)
        {
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
            asyncMethod().Wait();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
        }

        public T Run<T>(Func<Task<T>> asyncMethod)
        {
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
            return asyncMethod().GetAwaiter().GetResult();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
        }

        public Task SwitchToMainThreadAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
