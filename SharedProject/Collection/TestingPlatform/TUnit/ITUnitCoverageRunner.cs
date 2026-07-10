using System;
using System.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverage.Collection.TestingPlatform.TUnit
{
    internal interface ITUnitCoverageRunner
    {
        event EventHandler ReadyEvent;

        Task<bool> RunAsync(
            TUnitSettings tUnitSettings,
            bool hasCoverageExtension,
            bool showWindow = false,
            CancellationToken cancellationToken = default);
    }
}
