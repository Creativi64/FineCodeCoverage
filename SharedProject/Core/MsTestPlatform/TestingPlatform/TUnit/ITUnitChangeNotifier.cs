using System;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    internal interface ITUnitChangeNotifier
    {
        event EventHandler<ProjectAddedRemoved> ProjectAddedRemovedEvent;
        event EventHandler SolutionClosedEvent;
        event EventHandler SolutionOpenedEvent;
    }
}
