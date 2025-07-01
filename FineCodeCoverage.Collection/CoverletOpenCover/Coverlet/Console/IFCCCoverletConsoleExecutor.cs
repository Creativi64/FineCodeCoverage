using System.Threading;

namespace FineCodeCoverage.Collection.CoverletOpenCover.Coverlet.Console
{
    internal interface IFCCCoverletConsoleExecutor : ICoverletConsoleExecutor
    {
        void Initialize(string appDataFolder, CancellationToken cancellationToken);
    }
}
