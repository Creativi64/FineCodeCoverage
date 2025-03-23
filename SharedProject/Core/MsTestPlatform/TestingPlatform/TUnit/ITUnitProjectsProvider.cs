using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    internal interface ITUnitProjectsProvider
    {
        event EventHandler ReadyEvent;
        Task<List<ITUnitProject>> GetTUnitProjectsAsync(CancellationToken cancellationToken);
    }
}
