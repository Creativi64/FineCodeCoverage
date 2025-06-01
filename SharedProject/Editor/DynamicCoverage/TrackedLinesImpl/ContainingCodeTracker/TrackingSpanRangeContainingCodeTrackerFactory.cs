using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    [ExcludeFromCodeCoverage]
    [Export(typeof(ITrackingSpanRangeContainingCodeTrackerFactory))]
    internal class TrackingSpanRangeContainingCodeTrackerFactory : ITrackingSpanRangeContainingCodeTrackerFactory
    {
        private readonly IDirtyLineFactory dirtyLineFactory;

        [ImportingConstructor]
        public TrackingSpanRangeContainingCodeTrackerFactory(
            IDirtyLineFactory dirtyLineFactory
        ) => this.dirtyLineFactory = dirtyLineFactory;

        public IContainingCodeTracker CreateCoverageLines(ITrackingSpanRange trackingSpanRange, ITrackedCoverageLines trackedCoverageLines)
            => Wrap(trackingSpanRange, new CoverageCodeTracker(trackedCoverageLines, this.dirtyLineFactory));

        public IContainingCodeTracker CreateNotIncluded(ITrackingLine trackingLine, ITrackingSpanRange trackingSpanRange)
            => Wrap(trackingSpanRange, new TrackingLineTracker(trackingLine));

        public IContainingCodeTracker CreateOtherLines(ITrackingSpanRange trackingSpanRange)
            =>  Wrap(trackingSpanRange, new OtherLinesTracker());

        private static TrackingSpanRangeUpdatingTracker Wrap(
            ITrackingSpanRange trackingSpanRange, IUpdatableDynamicLines updatableDynamicLines
        ) => new TrackingSpanRangeUpdatingTracker(trackingSpanRange, updatableDynamicLines);
    }
}
