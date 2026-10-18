using System.Threading;
using FineCodeCoverage.VSAbstractions.Threading;

namespace FineCodeCoverageTests.TestHelpers
{
    internal sealed class TrackingCancellationSource : ICancellationTokenSource
    {
        private readonly CancellationTokenSource source = new CancellationTokenSource();

        public TrackingCancellationSource() => Token = source.Token;

        public CancellationToken Token { get; }

        public bool IsCancellationRequested => Token.IsCancellationRequested;

        public bool Disposed { get; private set; }

        public void Cancel() => source.Cancel();

        public void Dispose()
        {
            Disposed = true;
            source.Dispose();
        }
    }
}
