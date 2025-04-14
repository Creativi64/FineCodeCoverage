using System.Collections.Generic;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal class TrackingLineTracker : IUpdatableDynamicLines
    {
        private readonly ITrackingLine trackingLine;

        public TrackingLineTracker(
            ITrackingLine trackingLine
            ) => this.trackingLine = trackingLine;

        public IEnumerable<IDynamicLine> Lines => new List<IDynamicLine> { this.trackingLine.Line };

        public IEnumerable<int> GetUpdatedLineNumbers(
            TrackingSpanRangeProcessResult trackingSpanRangeProcessResult,
            ITextSnapshot currentSnapshot,
            List<SpanAndLineRange> newSpanAndLineRanges) => this.trackingLine.GetUpdatedLineNumbers(currentSnapshot);
    }
}
