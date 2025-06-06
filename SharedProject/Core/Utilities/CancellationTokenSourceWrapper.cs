using System.Threading;

namespace FineCodeCoverage.Core.Utilities
{
    internal sealed class CancellationTokenSourceWrapper : ICancellationTokenSource
    {
        private readonly CancellationTokenSource _cancellationTokenSource;

        public CancellationTokenSourceWrapper(CancellationTokenSource cancellationTokenSource)
            => _cancellationTokenSource = cancellationTokenSource;

        public CancellationToken Token => _cancellationTokenSource.Token;

        public bool IsCancellationRequested => _cancellationTokenSource.IsCancellationRequested;

        public void Cancel() => _cancellationTokenSource.Cancel();

        public void Dispose() => _cancellationTokenSource.Dispose();
    }
}
