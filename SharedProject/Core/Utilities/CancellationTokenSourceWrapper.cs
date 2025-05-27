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

        public CancellationToken Token => cancellationTokenSource.Token;

        public bool IsCancellationRequested => cancellationTokenSource.IsCancellationRequested;

        public void Cancel()
        {
            cancellationTokenSource.Cancel();
        }

        public void Dispose()
        {
            cancellationTokenSource.Dispose();
        }
    }

}
