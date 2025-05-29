using System.Threading;

namespace FineCodeCoverage.Core.Utilities
{
    internal class CancellationTokenSourceWrapper : ICancellationTokenSource
    {
        private readonly CancellationTokenSource cancellationTokenSource;

        public CancellationTokenSourceWrapper(CancellationTokenSource cancellationTokenSource)
        {
            this.cancellationTokenSource = cancellationTokenSource;
        }

        public CancellationToken Token => this.cancellationTokenSource.Token;

        public bool IsCancellationRequested => this.cancellationTokenSource.IsCancellationRequested;

        public void Cancel()
        {
            this.cancellationTokenSource.Cancel();
        }

        public void Dispose()
        {
            this.cancellationTokenSource.Dispose();
        }
    }

}
