using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal class CoverageCodeTracker : IUpdatableDynamicLines
    {
        private readonly ITrackedCoverageLines trackedCoverageLines;
        private readonly IDirtyLineFactory dirtyLineFactory;
        private ITrackingLine dirtyLine;

        public CoverageCodeTracker(
            ITrackedCoverageLines trackedCoverageLines,
            IDirtyLineFactory dirtyLineFactory
        )
        {
            this.trackedCoverageLines = trackedCoverageLines;
            this.dirtyLineFactory = dirtyLineFactory;
        }

        private List<int> CreateDirtyLineIfRequired(
            List<LineRange> newSpanChanges,
            List<LineRange> nonIntersecting,
            bool textChanged,
            ITextSnapshot currentSnapshot,
            ITrackingSpanRange trackingSpanRange
        ) => this.dirtyLine == null && textChanged && this.Intersected(newSpanChanges, nonIntersecting)
                ? this.CreateDirtyLine(currentSnapshot, trackingSpanRange)
                : null;

        private List<int> CreateDirtyLine(ITextSnapshot currentSnapshot, ITrackingSpanRange trackingSpanRange)
        {
            IDynamicCoberturaLine dynamicCoberturaLine = this.trackedCoverageLines.GetStartDynamicCoberturaLine();
            ITrackingSpan firstTrackingSpan = trackingSpanRange.GetFirstTrackingSpan();
            this.dirtyLine = this.dirtyLineFactory.Create(firstTrackingSpan, currentSnapshot, dynamicCoberturaLine);
            dynamicCoberturaLine?.CodeElement.IsDirty();
            return new int[] { this.dirtyLine.Line.Number }.Concat(this.trackedCoverageLines.Lines.Select(l => l.Number)).ToList();
        }

        private bool Intersected(
            List<LineRange> newSpanChanges,
            List<LineRange> nonIntersecting
        ) => nonIntersecting.Count < newSpanChanges.Count;

        public IEnumerable<int> GetUpdatedLineNumbers(
            TrackingSpanRangeProcessResult trackingSpanRangeProcessResult,
            ITextSnapshot currentSnapshot,
            List<LineRange> newSpanAndLineRanges
        )
        {
            List<int> changedLineNumbers = this.CreateDirtyLineIfRequired(
                    newSpanAndLineRanges,
                    trackingSpanRangeProcessResult.NonIntersectingSpans,
                    trackingSpanRangeProcessResult.TextChanged,
                    currentSnapshot,
                    trackingSpanRangeProcessResult.TrackingSpanRange
                );
            return changedLineNumbers ?? this.UpdateLines(currentSnapshot);
        }

        private IEnumerable<int> UpdateLines(ITextSnapshot currentSnapshot)
            => this.dirtyLine != null
                ? this.dirtyLine.GetUpdatedLineNumbers(currentSnapshot)
                : this.trackedCoverageLines.GetUpdatedLineNumbers(currentSnapshot);

        public void Deleted() => this.trackedCoverageLines.GetStartDynamicCoberturaLine()?.CodeElement.Deleted();

        public IEnumerable<IDynamicLine> Lines => this.dirtyLine != null ? new List<IDynamicLine> { this.dirtyLine.Line } : this.trackedCoverageLines.Lines;
    }
}
