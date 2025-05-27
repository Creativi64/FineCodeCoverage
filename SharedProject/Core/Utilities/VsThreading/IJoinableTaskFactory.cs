using System;
using System.Threading.Tasks;
using System.Threading;

namespace FineCodeCoverage.Core.Utilities.VsThreading
{
    internal interface IJoinableTaskFactory
    {
        T Run<T>(Func<Task<T>> asyncMethod);
        void Run(Func<Task> asyncMethod);
        Task SwitchToMainThreadAsync(CancellationToken cancellationToken = default);
    }
}
