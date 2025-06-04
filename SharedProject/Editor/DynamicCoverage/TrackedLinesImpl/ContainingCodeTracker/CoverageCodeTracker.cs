using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal class CoverageCodeTracker : IUpdatableDynamicLines
    {
        private readonly ITrackedCoverageLines _trackedCoverageLines;
        private readonly IDirtyLineFactory _dirtyLineFactory;
        private ITrackingLine _dirtyLine;

        public CoverageCodeTracker(
            ITrackedCoverageLines trackedCoverageLines,
            IDirtyLineFactory dirtyLineFactory
        )
        {
            this._trackedCoverageLines = trackedCoverageLines;
            this._dirtyLineFactory = dirtyLineFactory;
        }

        private List<int> CreateDirtyLineIfRequired(
            List<LineRange> newSpanChanges,
            List<LineRange> nonIntersecting,
            bool textChanged,
            ITrackingSpanRange trackingSpanRange
        ) => this._dirtyLine == null && textChanged && Intersected(newSpanChanges, nonIntersecting)
                ? this.CreateDirtyLine(trackingSpanRange)
                : null;

        private List<int> CreateDirtyLine(ITrackingSpanRange trackingSpanRange)
        {
            FirstTrackedCoverageLineInfo firstTrackedCoverageLineInfo = this._trackedCoverageLines.GetFirstTrackedCoverageLineInfo();
            IDynamicCoberturaLine dynamicCoberturaLine = firstTrackedCoverageLineInfo.DynamicCoberturaLine;
            ITrackingSpan firstTrackingSpan = trackingSpanRange.GetFirstTrackingSpan();
            this._dirtyLine = this._dirtyLineFactory.Create(firstTrackingSpan, firstTrackedCoverageLineInfo.OriginalLineNumber, dynamicCoberturaLine);
            dynamicCoberturaLine?.CodeElement.IsDirty();
            return new int[] { this._dirtyLine.Line.LineNumber }.Concat(this._trackedCoverageLines.Lines.Select(l => l.LineNumber)).ToList();
        }

        private static bool Intersected(
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
                    trackingSpanRangeProcessResult.TrackingSpanRange
                );
            return changedLineNumbers ?? this.UpdateLines(currentSnapshot);
        }

        private IEnumerable<int> UpdateLines(ITextSnapshot currentSnapshot)
            => this._dirtyLine != null
                ? this._dirtyLine.GetUpdatedLineNumbers(currentSnapshot)
                : this._trackedCoverageLines.GetUpdatedLineNumbers(currentSnapshot);

        public void Deleted() => this._trackedCoverageLines.GetFirstTrackedCoverageLineInfo().DynamicCoberturaLine?.CodeElement.Deleted();

        public IEnumerable<IDynamicLine> Lines => this._dirtyLine != null ? new List<IDynamicLine> { this._dirtyLine.Line } : this._trackedCoverageLines.Lines;
    }
}