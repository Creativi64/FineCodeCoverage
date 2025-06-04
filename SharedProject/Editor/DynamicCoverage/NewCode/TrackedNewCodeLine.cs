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
            _line = new DynamicLine(lineNumber, DynamicCoverageType.NewLine);
            _lineTracker = lineTracker;
            _trackingSpan = trackingSpan;
        }

        public IDynamicLine Line => _line;

        public string GetText(ITextSnapshot currentSnapshot)
            => _lineTracker.GetTrackedLineInfo(_trackingSpan, currentSnapshot, true).LineText;

        public TrackedNewCodeLineUpdate Update(ITextSnapshot currentSnapshot)
        {
            int oldLineNumber = _line.LineNumber;
            TrackedLineInfo trackedLineInfo = _lineTracker.GetTrackedLineInfo(
                _trackingSpan, currentSnapshot, true);
            _line.LineNumber = trackedLineInfo.LineNumber;
            return new TrackedNewCodeLineUpdate(trackedLineInfo.LineText, _line.LineNumber, oldLineNumber);
        }
    }
}
