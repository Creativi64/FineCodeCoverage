using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal class TrackedNewCodeLine : ITrackedNewCodeLine
    {
        private readonly ITrackingSpan _trackingSpan;
        private readonly DynamicLine _line;
        private readonly ILineTracker _lineTracker;

        public TrackedNewCodeLine(ITrackingSpan trackingSpan, int lineNumber, ILineTracker lineTracker)
        {
            this._line = new DynamicLine(lineNumber, DynamicCoverageType.NewLine);
            this._lineTracker = lineTracker;
            this._trackingSpan = trackingSpan;
        }

        public IDynamicLine Line => this._line;

        public string GetText(ITextSnapshot currentSnapshot)
            => this._lineTracker.GetTrackedLineInfo(this._trackingSpan, currentSnapshot, true).LineText;

        public TrackedNewCodeLineUpdate Update(ITextSnapshot currentSnapshot)
        {
            int oldLineNumber = this._line.LineNumber;
            TrackedLineInfo trackedLineInfo = this._lineTracker.GetTrackedLineInfo(
                this._trackingSpan, currentSnapshot, true);
            this._line.LineNumber = trackedLineInfo.LineNumber;
            return new TrackedNewCodeLineUpdate(trackedLineInfo.LineText, this._line.LineNumber, oldLineNumber);
        }
    }
}