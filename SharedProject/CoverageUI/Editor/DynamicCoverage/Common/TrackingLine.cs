using System.Collections.Generic;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal sealed class TrackingLine : ITrackingLine
    {
        private readonly ITrackingSpan _startTrackingSpan;
        private readonly ILineTracker _lineTracker;

        public IDynamicLine Line { get; }

        public TrackingLine(
            ITrackingSpan startTrackingSpan,
            ITextSnapshot currentSnapshot,
            ILineTracker lineTracker,
            DynamicCoverageType dynamicCoverageType)
        {
            _startTrackingSpan = startTrackingSpan;
            _lineTracker = lineTracker;
            int lineNumber = _lineTracker.GetLineNumber(_startTrackingSpan, currentSnapshot, false);
            Line = new DynamicLine(lineNumber, dynamicCoverageType);
        }

        public TrackingLine(
            ITrackingSpan startTrackingSpan,
            ILineTracker lineTracker,
            DynamicCoverageType dynamicCoverageType,
            int originalLineNumber)
        {
            _lineTracker = lineTracker;
            _startTrackingSpan = startTrackingSpan;
            Line = new DynamicLine(originalLineNumber, dynamicCoverageType);
        }

        private void UpdateLineNumber(ITextSnapshot currentSnapshot)
        {
            int lineNumber = _lineTracker.GetLineNumber(_startTrackingSpan, currentSnapshot, false);
            (Line as DynamicLine).LineNumber = lineNumber;
        }

        public List<int> GetUpdatedLineNumbers(ITextSnapshot currentSnapshot)
        {
            int currentFirstLineNumber = Line.LineNumber;
            UpdateLineNumber(currentSnapshot);
            bool updated = currentFirstLineNumber != Line.LineNumber;
            return updated ? new List<int> { currentFirstLineNumber, Line.LineNumber } : new List<int>();
        }
    }
}
