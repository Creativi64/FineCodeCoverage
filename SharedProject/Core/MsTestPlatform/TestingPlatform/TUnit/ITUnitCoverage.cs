using System;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    interface ITUnitCoverage
    {
        event EventHandler<bool> CollectingChangedEvent;
        event EventHandler ReadyEvent;
        void CollectCoverage();
        void Cancel();
    }
}
