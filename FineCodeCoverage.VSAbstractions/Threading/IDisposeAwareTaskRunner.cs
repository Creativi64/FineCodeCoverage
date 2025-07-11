using System;
using System.Threading.Tasks;

namespace FineCodeCoverage.VSAbstractions.Threading
{
    public interface IDisposeAwareTaskRunner
    {
        void RunAsyncFunc(Func<Task> taskProvider);

        ICancellationTokenSource CreateLinkedTokenSource();

        bool IsVsShutdown { get; }
    }
}
