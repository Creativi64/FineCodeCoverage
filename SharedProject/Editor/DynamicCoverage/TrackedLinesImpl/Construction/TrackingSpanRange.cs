using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal class TrackingSpanRange : ITrackingSpanRange
    {
        private readonly ITrackingSpan _startTrackingSpan;
        private readonly ITrackingSpan _endTrackingSpan;
        private readonly ILineTracker _lineTracker;
        private string _lastRangeText;
        private CodeSpanRange _codeSpanRange;

        public TrackingSpanRange(
            ITrackingSpan startTrackingSpan,
            ITrackingSpan endTrackingSpan,
            ITextSnapshot currentSnapshot,
            ILineTracker lineTracker)
        {
            _startTrackingSpan = startTrackingSpan;
            _endTrackingSpan = endTrackingSpan;
            _lineTracker = lineTracker;
            (SnapshotSpan currentStartSpan, SnapshotSpan currentEndSpan) = GetCurrentRange(currentSnapshot);
            SetRangeText(currentSnapshot, currentStartSpan, currentEndSpan);
        }

        private (SnapshotSpan, SnapshotSpan) GetCurrentRange(ITextSnapshot currentSnapshot)
        {
            SnapshotSpan currentStartSpan = _startTrackingSpan.GetSpan(currentSnapshot);
            SnapshotSpan currentEndSpan = _endTrackingSpan.GetSpan(currentSnapshot);
            int startLineNumber = _lineTracker.GetLineNumber(_startTrackingSpan, currentSnapshot, false);
            int endLineNumber = _lineTracker.GetLineNumber(_endTrackingSpan, currentSnapshot, true);
            _codeSpanRange = new CodeSpanRange(startLineNumber, endLineNumber);
            return (currentStartSpan, currentEndSpan);
        }

        private void SetRangeText(ITextSnapshot currentSnapshot, SnapshotSpan currentFirstSpan, SnapshotSpan currentEndSpan)
            => _lastRangeText = currentSnapshot.GetText(new Span(currentFirstSpan.Start, currentEndSpan.End - currentFirstSpan.Start));

        public TrackingSpanRangeProcessResult Process(ITextSnapshot currentSnapshot, List<LineRange> newSpanAndLineRanges)
        {
            (SnapshotSpan currentFirstSpan, SnapshotSpan currentEndSpan) = GetCurrentRange(currentSnapshot);
            (bool isEmpty, bool textChanged) = GetTextChangeInfo(currentSnapshot, currentFirstSpan, currentEndSpan);
            List<LineRange> nonIntersecting = GetNonIntersecting(currentSnapshot, currentFirstSpan, currentEndSpan, newSpanAndLineRanges);
            return new TrackingSpanRangeProcessResult(this, nonIntersecting, isEmpty, textChanged);
        }

        private (bool isEmpty, bool textChanged) GetTextChangeInfo(ITextSnapshot currentSnapshot, SnapshotSpan currentFirstSpan, SnapshotSpan currentEndSpan)
        {
            string previousRangeText = _lastRangeText;
            SetRangeText(currentSnapshot, currentFirstSpan, currentEndSpan);
            bool textChanged = previousRangeText != _lastRangeText;
            bool isEmpty = string.IsNullOrWhiteSpace(_lastRangeText);
            return (isEmpty, textChanged);
        }

        private static List<LineRange> GetNonIntersecting(
            ITextSnapshot currentSnapshot, SnapshotSpan currentFirstSpan, SnapshotSpan currentEndSpan, List<LineRange> newSpanAndLineRanges)
        {
            int currentFirstTrackedLineNumber = currentSnapshot.GetLineNumberFromPosition(currentFirstSpan.End);
            int currentEndTrackedLineNumber = currentSnapshot.GetLineNumberFromPosition(currentEndSpan.End);
            return newSpanAndLineRanges.Where(
                spanAndLineNumber => IsOutsideRange(
                    currentFirstTrackedLineNumber,
                    currentEndTrackedLineNumber,
                    spanAndLineNumber.StartLineNumber)
                &&
                IsOutsideRange(currentFirstTrackedLineNumber, currentEndTrackedLineNumber, spanAndLineNumber.EndLineNumber)).ToList();
        }

        private static bool IsOutsideRange(int firstLineNumber, int endLineNumber, int spanLineNumber)
            => spanLineNumber < firstLineNumber || spanLineNumber > endLineNumber;

        public ITrackingSpan GetFirstTrackingSpan() => _startTrackingSpan;

        public CodeSpanRange ToCodeSpanRange() => _codeSpanRange;
    }
}
