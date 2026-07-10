using System;
using System.Threading;

namespace FineCodeCoverage.VSAbstractions.Threading
{
    public interface ICancellationTokenSource : IDisposable
    {
        CancellationToken Token { get; }

        bool IsCancellationRequested { get; }

        void Cancel();
    }
}
