using System.Collections.Generic;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal class TrackingLine : ITrackingLine
    {
        private readonly ITrackingSpan startTrackingSpan;
        private readonly ILineTracker lineTracker;

        public IDynamicLine Line { get; }
        public TrackingLine(
            ITrackingSpan startTrackingSpan,
            ITextSnapshot currentSnapshot,
            ILineTracker lineTracker,
            DynamicCoverageType dynamicCoverageType)
        {
            this.startTrackingSpan = startTrackingSpan;
            this.lineTracker = lineTracker;
            int lineNumber = this.lineTracker.GetLineNumber(this.startTrackingSpan, currentSnapshot, false);
            this.Line = new DynamicLine(lineNumber, dynamicCoverageType);
        }

        public TrackingLine(
            ITrackingSpan startTrackingSpan,
            ILineTracker lineTracker,
            DynamicCoverageType dynamicCoverageType,
            int originalLineNumber)
        {
            this.lineTracker = lineTracker;
            this.startTrackingSpan = startTrackingSpan;
            this.Line = new DynamicLine(originalLineNumber, dynamicCoverageType);
        }

        private void UpdateLineNumber(ITextSnapshot currentSnapshot)
        {
            int lineNumber = this.lineTracker.GetLineNumber(this.startTrackingSpan, currentSnapshot, false);
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
