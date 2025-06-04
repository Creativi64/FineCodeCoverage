using System.Threading;

namespace FineCodeCoverage.Core.Utilities
{
    internal class CancellationTokenSourceWrapper : ICancellationTokenSource
    {
        private readonly CancellationTokenSource _cancellationTokenSource;

        public CancellationTokenSourceWrapper(CancellationTokenSource cancellationTokenSource)
            => this._cancellationTokenSource = cancellationTokenSource;

        public CancellationToken Token => this._cancellationTokenSource.Token;

        public bool IsCancellationRequested => this._cancellationTokenSource.IsCancellationRequested;

        public void Cancel() => this._cancellationTokenSource.Cancel();

        public void Dispose() => this._cancellationTokenSource.Dispose();
    }
}