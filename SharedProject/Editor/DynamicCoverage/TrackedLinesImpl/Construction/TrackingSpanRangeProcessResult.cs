using System.Collections.Generic;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal class TrackingSpanRangeProcessResult
    {
        public TrackingSpanRangeProcessResult(
            ITrackingSpanRange trackingSpanRange,
            List<LineRange> nonIntersectingSpans,
            bool isEmpty,
            bool textChanged)
        {
            TrackingSpanRange = trackingSpanRange;
            NonIntersectingSpans = nonIntersectingSpans;
            IsEmpty = isEmpty;
            TextChanged = textChanged;
        }

        public ITrackingSpanRange TrackingSpanRange { get; }

        public List<LineRange> NonIntersectingSpans { get; }

        public bool IsEmpty { get; }

        public bool TextChanged { get; }
    }
}
