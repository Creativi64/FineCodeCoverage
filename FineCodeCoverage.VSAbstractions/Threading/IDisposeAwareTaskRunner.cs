using System;
using System.Threading.Tasks;

namespace FineCodeCoverage.Core.Utilities
{
    public interface IDisposeAwareTaskRunner
    {
        void RunAsyncFunc(Func<Task> taskProvider);

        ICancellationTokenSource CreateLinkedTokenSource();

        bool IsVsShutdown { get; }
    }
}
