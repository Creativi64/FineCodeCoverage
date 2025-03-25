using System;
using System.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    internal interface ITUnitRunner
    {
        event EventHandler ReadyEvent;
        Task<bool> RunAsync(
            string exePath,
            string settingsPath,
            string outputpath,
            bool hasCoverageExtension,
            bool showWindow = false,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
