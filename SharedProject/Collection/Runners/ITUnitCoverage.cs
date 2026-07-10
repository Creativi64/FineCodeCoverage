using System;

namespace FineCodeCoverage.Collection.Runners
{
    internal interface ITUnitCoverage
    {
        bool Ready { get; }

        event EventHandler<bool> CollectingChangedEvent;

        event EventHandler ReadyEvent;

        void CollectCoverage();

        void Cancel();
    }
}
