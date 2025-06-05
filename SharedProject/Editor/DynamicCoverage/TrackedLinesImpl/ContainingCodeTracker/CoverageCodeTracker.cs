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
            IDirtyLineFactory dirtyLineFactory)
        {
            _trackedCoverageLines = trackedCoverageLines;
            _dirtyLineFactory = dirtyLineFactory;
        }

        private List<int> CreateDirtyLineIfRequired(
            List<LineRange> newSpanChanges,
            List<LineRange> nonIntersecting,
            bool textChanged,
            ITrackingSpanRange trackingSpanRange)
            => _dirtyLine == null && textChanged && Intersected(newSpanChanges, nonIntersecting)
                ? CreateDirtyLine(trackingSpanRange)
                : null;

        private List<int> CreateDirtyLine(ITrackingSpanRange trackingSpanRange)
        {
            FirstTrackedCoverageLineInfo firstTrackedCoverageLineInfo = _trackedCoverageLines.GetFirstTrackedCoverageLineInfo();
            IDynamicCoberturaLine dynamicCoberturaLine = firstTrackedCoverageLineInfo.DynamicCoberturaLine;
            ITrackingSpan firstTrackingSpan = trackingSpanRange.GetFirstTrackingSpan();
            _dirtyLine = _dirtyLineFactory.Create(firstTrackingSpan, firstTrackedCoverageLineInfo.OriginalLineNumber, dynamicCoberturaLine);
            dynamicCoberturaLine?.CodeElement.IsDirty();
            return new int[] { _dirtyLine.Line.LineNumber }.Concat(_trackedCoverageLines.Lines.Select(l => l.LineNumber)).ToList();
        }

        private static bool Intersected(
            List<LineRange> newSpanChanges,
            List<LineRange> nonIntersecting) => nonIntersecting.Count < newSpanChanges.Count;

        public IEnumerable<int> GetUpdatedLineNumbers(
            TrackingSpanRangeProcessResult trackingSpanRangeProcessResult,
            ITextSnapshot currentSnapshot,
            List<LineRange> newSpanAndLineRanges)
        {
            List<int> changedLineNumbers = CreateDirtyLineIfRequired(
                    newSpanAndLineRanges,
                    trackingSpanRangeProcessResult.NonIntersectingSpans,
                    trackingSpanRangeProcessResult.TextChanged,
                    trackingSpanRangeProcessResult.TrackingSpanRange);
            return changedLineNumbers ?? UpdateLines(currentSnapshot);
        }

        private IEnumerable<int> UpdateLines(ITextSnapshot currentSnapshot)
            => _dirtyLine != null
                ? _dirtyLine.GetUpdatedLineNumbers(currentSnapshot)
                : _trackedCoverageLines.GetUpdatedLineNumbers(currentSnapshot);

        public void Deleted() => _trackedCoverageLines.GetFirstTrackedCoverageLineInfo().DynamicCoberturaLine?.CodeElement.Deleted();

        public IEnumerable<IDynamicLine> Lines => _dirtyLine != null ? new List<IDynamicLine> { _dirtyLine.Line } : _trackedCoverageLines.Lines;
    }
}
