using System;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    interface ITUnitCoverage
    {
        event EventHandler<bool> CollectingChanged;
        void CollectCoverage();
        void Cancel();
    }
}
