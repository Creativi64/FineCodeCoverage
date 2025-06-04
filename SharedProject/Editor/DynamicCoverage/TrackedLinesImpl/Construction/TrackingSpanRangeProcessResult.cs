using System.Collections.Generic;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal class TrackingSpanRangeProcessResult
    {
        public TrackingSpanRangeProcessResult(
            ITrackingSpanRange trackingSpanRange,
            List<LineRange> nonIntersectingSpans,
            bool isEmpty,
            bool textChanged
        )
        {
            this.TrackingSpanRange = trackingSpanRange;
            this.NonIntersectingSpans = nonIntersectingSpans;
            this.IsEmpty = isEmpty;
            this.TextChanged = textChanged;
        }

        public ITrackingSpanRange TrackingSpanRange { get; }

        public List<LineRange> NonIntersectingSpans { get; }

        public bool IsEmpty { get; }

        public bool TextChanged { get; }
    }
}
