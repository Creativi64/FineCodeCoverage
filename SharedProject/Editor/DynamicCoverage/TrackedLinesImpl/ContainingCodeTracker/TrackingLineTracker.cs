using System.Collections.Generic;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal class TrackingLineTracker : IUpdatableDynamicLines
    {
        private readonly ITrackingLine _trackingLine;

        public TrackingLineTracker(ITrackingLine trackingLine) => this._trackingLine = trackingLine;

        public IEnumerable<IDynamicLine> Lines => new List<IDynamicLine> { this._trackingLine.Line };

        public void Deleted() { }

        public IEnumerable<int> GetUpdatedLineNumbers(
            TrackingSpanRangeProcessResult trackingSpanRangeProcessResult,
            ITextSnapshot currentSnapshot,
            List<LineRange> newSpanAndLineRanges) => this._trackingLine.GetUpdatedLineNumbers(currentSnapshot);
    }
}
