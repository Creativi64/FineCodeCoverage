using System;
using System.Threading.Tasks;

namespace FineCodeCoverage.Core.Utilities
{
    internal interface IDisposeAwareTaskRunner
    {
        void RunAsyncFunc(Func<Task> taskProvider);

        ICancellationTokenSource CreateLinkedTokenSource();

        bool IsVsShutdown { get; }
    }
}