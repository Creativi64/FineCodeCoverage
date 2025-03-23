using System.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    internal interface ITUnitRunner
    {
        // todo change to return exit code too for logging
        Task<bool> RunAsync(
            string exePath,
            string settingsPath,
            string outputpath,
            bool showWindow = false,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
