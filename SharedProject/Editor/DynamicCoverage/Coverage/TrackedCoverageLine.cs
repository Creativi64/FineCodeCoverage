using System;
using System.Collections.Generic;
using System.Linq;
using FineCodeCoverage.Engine.ReportGenerator;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal class TrackedCoverageLine : ITrackedCoverageLine
    {
        private readonly ITrackingSpan trackingSpan;
        private readonly ILineTracker lineTracker;
        private readonly DynamicLine line;
        public IDynamicLine Line => this.line;

        public IDynamicCoberturaLine DynamicCoberturaLine { get; }

        private readonly Action<int> updateDynamicCoberturaLine = (_) => { };

        public TrackedCoverageLine(ITrackingSpan trackingSpan, ICoberturaLine coberturaLine, ILineTracker lineTracker)
        {
            this.line = DynamicLine.FromCoberturaLine(coberturaLine);
            this.trackingSpan = trackingSpan;
            this.lineTracker = lineTracker;
            if (coberturaLine is IDynamicCoberturaLine dynamicCoberturaLine)
            {
                this.DynamicCoberturaLine = dynamicCoberturaLine;
                this.updateDynamicCoberturaLine = (newLineNumber) => dynamicCoberturaLine.LineMoved(newLineNumber);
            }
        }

        public List<int> GetUpdateLineNumbers(ITextSnapshot currentSnapshot)
        {
            int previousLineNumber = this.Line.LineNumber;
            int newLineNumber = this.lineTracker.GetLineNumber(this.trackingSpan, currentSnapshot, true);
            if (newLineNumber != previousLineNumber)
            {
                this.line.LineNumber = newLineNumber;
                this.updateDynamicCoberturaLine(newLineNumber);
                return new List<int> { previousLineNumber, newLineNumber };

            }

            return Enumerable.Empty<int>().ToList();
        }
    }
}
