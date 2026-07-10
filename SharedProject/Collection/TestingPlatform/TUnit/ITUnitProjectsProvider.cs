using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverage.Collection.TestingPlatform.TUnit
{
    internal interface ITUnitProjectsProvider
    {
        bool Ready { get; }

        event EventHandler ReadyEvent;

        Task<List<ITUnitProject>> GetTUnitProjectsAsync(CancellationToken cancellationToken);
    }
}
