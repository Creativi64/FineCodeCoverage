using System;
using System.Threading;

namespace FineCodeCoverage.Core.Utilities
{
    internal interface ICancellationTokenSource : IDisposable
    {
        CancellationToken Token { get; }

        bool IsCancellationRequested { get; }

        void Cancel();
    }
}
