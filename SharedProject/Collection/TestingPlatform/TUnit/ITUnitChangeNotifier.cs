using System;

namespace FineCodeCoverage.Collection.TestingPlatform.TUnit
{
    internal interface ITUnitChangeNotifier
    {
        event EventHandler<ProjectAddedRemoved> ProjectAddedRemovedEvent;

        event EventHandler SolutionClosedEvent;

        event EventHandler SolutionOpenedEvent;
    }
}
