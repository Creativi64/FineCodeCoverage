using System.Collections.Generic;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal class TrackingLine : ITrackingLine
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
            this._startTrackingSpan = startTrackingSpan;
            this._lineTracker = lineTracker;
            int lineNumber = this._lineTracker.GetLineNumber(this._startTrackingSpan, currentSnapshot, false);
            this.Line = new DynamicLine(lineNumber, dynamicCoverageType);
        }

        public TrackingLine(
            ITrackingSpan startTrackingSpan,
            ILineTracker lineTracker,
            DynamicCoverageType dynamicCoverageType,
            int originalLineNumber)
        {
            this._lineTracker = lineTracker;
            this._startTrackingSpan = startTrackingSpan;
            this.Line = new DynamicLine(originalLineNumber, dynamicCoverageType);
        }

        private void UpdateLineNumber(ITextSnapshot currentSnapshot)
        {
            int lineNumber = this._lineTracker.GetLineNumber(this._startTrackingSpan, currentSnapshot, false);
            (this.Line as DynamicLine).LineNumber = lineNumber;
        }

        public List<int> GetUpdatedLineNumbers(ITextSnapshot currentSnapshot)
        {
            int currentFirstLineNumber = this.Line.LineNumber;
            this.UpdateLineNumber(currentSnapshot);
            bool updated = currentFirstLineNumber != this.Line.LineNumber;
            return updated ? new List<int> { currentFirstLineNumber, this.Line.LineNumber } : new List<int>();
        }
    }
}
