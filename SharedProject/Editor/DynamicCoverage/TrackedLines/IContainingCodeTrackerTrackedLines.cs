using System.Collections.Generic;
using FineCodeCoverage.Editor.DynamicCoverage.Management;
using FineCodeCoverage.Editor.DynamicCoverage.NewCode;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal interface IContainingCodeTrackerTrackedLines : ITrackedLines
    {
        IReadOnlyList<IContainingCodeTracker> ContainingCodeTrackers { get; }

        INewCodeTracker NewCodeTracker { get; }
    }
}
