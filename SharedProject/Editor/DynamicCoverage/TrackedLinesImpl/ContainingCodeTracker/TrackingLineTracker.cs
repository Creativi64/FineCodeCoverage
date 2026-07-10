using System.Collections.Generic;
using FineCodeCoverage.Editor.DynamicCoverage.Common;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal sealed class TrackingLineTracker : IUpdatableDynamicLines
    {
        private readonly ITrackingLine _trackingLine;

        public TrackingLineTracker(ITrackingLine trackingLine) => _trackingLine = trackingLine;

        public IEnumerable<IDynamicLine> Lines => new List<IDynamicLine> { _trackingLine.Line };

        public void Deleted()
        {
        }

        public IEnumerable<int> GetUpdatedLineNumbers(
            TrackingSpanRangeProcessResult trackingSpanRangeProcessResult,
            ITextSnapshot currentSnapshot,
            List<LineRange> newSpanAndLineRanges) => _trackingLine.GetUpdatedLineNumbers(currentSnapshot);
    }
}
