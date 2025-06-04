using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal class TrackingSpanRangeUpdatingTracker : IContainingCodeTracker
    {
        private readonly ITrackingSpanRange _trackingSpanRange;
        private readonly IUpdatableDynamicLines _updatableDynamicLines;

        public TrackingSpanRangeUpdatingTracker(
            ITrackingSpanRange trackingSpanRange,
            IUpdatableDynamicLines updatableDynamicLines
        )
        {
            _trackingSpanRange = trackingSpanRange;
            _updatableDynamicLines = updatableDynamicLines;
        }

        public IEnumerable<IDynamicLine> Lines => _updatableDynamicLines.Lines;

        public void Deleted() => _updatableDynamicLines.Deleted();

        public ContainingCodeTrackerState GetState()
            => new ContainingCodeTrackerState(_trackingSpanRange.ToCodeSpanRange(), Lines);

        public IContainingCodeTrackerProcessResult ProcessChanges(ITextSnapshot currentSnapshot, List<LineRange> newSpanAndLineRanges)
        {
            TrackingSpanRangeProcessResult trackingSpanRangeProcessResult = _trackingSpanRange.Process(currentSnapshot, newSpanAndLineRanges);
            List<LineRange> nonIntersectingSpans = trackingSpanRangeProcessResult.NonIntersectingSpans;
            if (trackingSpanRangeProcessResult.IsEmpty)
            {
                IEnumerable<int> lines = _updatableDynamicLines.Lines.Select(l => l.LineNumber);
                return new ContainingCodeTrackerProcessResult(lines, nonIntersectingSpans, true);
            }

            IEnumerable<int> changedLines = _updatableDynamicLines.GetUpdatedLineNumbers(trackingSpanRangeProcessResult, currentSnapshot, newSpanAndLineRanges);
            return new ContainingCodeTrackerProcessResult(changedLines, nonIntersectingSpans, false);
        }
    }
}
