namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal interface ITrackingSpanRangeContainingCodeTrackerFactory
    {
        IContainingCodeTracker CreateCoverageLines(ITrackingSpanRange trackingSpanRange, ITrackedCoverageLines trackedCoverageLines);
        IContainingCodeTracker CreateNotIncluded(ITrackingLine trackingLine, ITrackingSpanRange trackingSpanRange);
        IContainingCodeTracker CreateOtherLines(ITrackingSpanRange trackingSpanRange);
    }
}
