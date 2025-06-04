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
            ILineTracker lineTracker
        )
        {
            this._startTrackingSpan = startTrackingSpan;
            this._endTrackingSpan = endTrackingSpan;
            this._lineTracker = lineTracker;
            (SnapshotSpan currentStartSpan, SnapshotSpan currentEndSpan) = this.GetCurrentRange(currentSnapshot);
            this.SetRangeText(currentSnapshot, currentStartSpan, currentEndSpan);
        }

        private (SnapshotSpan, SnapshotSpan) GetCurrentRange(ITextSnapshot currentSnapshot)
        {
            SnapshotSpan currentStartSpan = this._startTrackingSpan.GetSpan(currentSnapshot);
            SnapshotSpan currentEndSpan = this._endTrackingSpan.GetSpan(currentSnapshot);
            int startLineNumber = this._lineTracker.GetLineNumber(this._startTrackingSpan, currentSnapshot, false);
            int endLineNumber = this._lineTracker.GetLineNumber(this._endTrackingSpan, currentSnapshot, true);
            this._codeSpanRange = new CodeSpanRange(startLineNumber, endLineNumber);
            return (currentStartSpan, currentEndSpan);
        }

        private void SetRangeText(ITextSnapshot currentSnapshot, SnapshotSpan currentFirstSpan, SnapshotSpan currentEndSpan)
            => this._lastRangeText = currentSnapshot.GetText(new Span(currentFirstSpan.Start, currentEndSpan.End - currentFirstSpan.Start));

        public TrackingSpanRangeProcessResult Process(ITextSnapshot currentSnapshot, List<LineRange> newSpanAndLineRanges)
        {
            (SnapshotSpan currentFirstSpan, SnapshotSpan currentEndSpan) = this.GetCurrentRange(currentSnapshot);
            (bool isEmpty, bool textChanged) = this.GetTextChangeInfo(currentSnapshot, currentFirstSpan, currentEndSpan);
            List<LineRange> nonIntersecting = GetNonIntersecting(currentSnapshot, currentFirstSpan, currentEndSpan, newSpanAndLineRanges);
            return new TrackingSpanRangeProcessResult(this, nonIntersecting, isEmpty, textChanged);
        }

        private (bool isEmpty, bool textChanged) GetTextChangeInfo(ITextSnapshot currentSnapshot, SnapshotSpan currentFirstSpan, SnapshotSpan currentEndSpan)
        {
            string previousRangeText = this._lastRangeText;
            this.SetRangeText(currentSnapshot, currentFirstSpan, currentEndSpan);
            bool textChanged = previousRangeText != this._lastRangeText;
            bool isEmpty = string.IsNullOrWhiteSpace(this._lastRangeText);
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

        public ITrackingSpan GetFirstTrackingSpan() => this._startTrackingSpan;

        public CodeSpanRange ToCodeSpanRange() => this._codeSpanRange;

    }
}
