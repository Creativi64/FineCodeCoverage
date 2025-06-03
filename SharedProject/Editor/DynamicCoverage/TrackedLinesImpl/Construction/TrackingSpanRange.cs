using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal class TrackingSpanRange : ITrackingSpanRange
    {
        private readonly ITrackingSpan startTrackingSpan;
        private readonly ITrackingSpan endTrackingSpan;
        private readonly ILineTracker lineTracker;
        private string lastRangeText;
        private CodeSpanRange codeSpanRange;

        public TrackingSpanRange(
            ITrackingSpan startTrackingSpan,
            ITrackingSpan endTrackingSpan,
            ITextSnapshot currentSnapshot,
            ILineTracker lineTracker
        )
        {
            this.startTrackingSpan = startTrackingSpan;
            this.endTrackingSpan = endTrackingSpan;
            this.lineTracker = lineTracker;
            (SnapshotSpan currentStartSpan, SnapshotSpan currentEndSpan) = this.GetCurrentRange(currentSnapshot);
            this.SetRangeText(currentSnapshot, currentStartSpan, currentEndSpan);
        }

        private (SnapshotSpan, SnapshotSpan) GetCurrentRange(ITextSnapshot currentSnapshot)
        {
            SnapshotSpan currentStartSpan = this.startTrackingSpan.GetSpan(currentSnapshot);
            SnapshotSpan currentEndSpan = this.endTrackingSpan.GetSpan(currentSnapshot);
            int startLineNumber = this.lineTracker.GetLineNumber(this.startTrackingSpan, currentSnapshot, false);
            int endLineNumber = this.lineTracker.GetLineNumber(this.endTrackingSpan, currentSnapshot, true);
            this.codeSpanRange = new CodeSpanRange(startLineNumber, endLineNumber);
            return (currentStartSpan, currentEndSpan);
        }

        private void SetRangeText(ITextSnapshot currentSnapshot, SnapshotSpan currentFirstSpan, SnapshotSpan currentEndSpan)
            => this.lastRangeText = currentSnapshot.GetText(new Span(currentFirstSpan.Start, currentEndSpan.End - currentFirstSpan.Start));

        public TrackingSpanRangeProcessResult Process(ITextSnapshot currentSnapshot, List<LineRange> newSpanAndLineRanges)
        {
            (SnapshotSpan currentFirstSpan, SnapshotSpan currentEndSpan) = this.GetCurrentRange(currentSnapshot);
            (bool isEmpty, bool textChanged) = this.GetTextChangeInfo(currentSnapshot, currentFirstSpan, currentEndSpan);
            List<LineRange> nonIntersecting = GetNonIntersecting(currentSnapshot, currentFirstSpan, currentEndSpan, newSpanAndLineRanges);
            return new TrackingSpanRangeProcessResult(this, nonIntersecting, isEmpty, textChanged);
        }

        private (bool isEmpty, bool textChanged) GetTextChangeInfo(ITextSnapshot currentSnapshot, SnapshotSpan currentFirstSpan, SnapshotSpan currentEndSpan)
        {
            string previousRangeText = this.lastRangeText;
            this.SetRangeText(currentSnapshot, currentFirstSpan, currentEndSpan);
            bool textChanged = previousRangeText != this.lastRangeText;
            bool isEmpty = string.IsNullOrWhiteSpace(this.lastRangeText);
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

        public ITrackingSpan GetFirstTrackingSpan() => this.startTrackingSpan;

        public CodeSpanRange ToCodeSpanRange() => this.codeSpanRange;

    }
}