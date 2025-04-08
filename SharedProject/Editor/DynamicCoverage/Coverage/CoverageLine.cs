using System.Collections.Generic;
using System.Linq;
using FineCodeCoverage.Engine.Model;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal class CoverageLine : ICoverageLine
    {
        private readonly ITrackingSpan trackingSpan;
        private readonly ILineTracker lineTracker;
        private readonly DynamicLine line;
        public IDynamicLine Line => this.line;

        public CoverageLine(ITrackingSpan trackingSpan, ICoberturaLine coberturaLine, ILineTracker lineTracker)
        {
            this.line = DynamicLine.FromCoberturaLine(coberturaLine);
            this.trackingSpan = trackingSpan;
            this.lineTracker = lineTracker;
        }

        public List<int> GetUpdateLineNumbers(ITextSnapshot currentSnapshot)
        {
            int previousLineNumber = this.Line.Number;
            int newLineNumber = this.lineTracker.GetLineNumber(this.trackingSpan, currentSnapshot, true);
            if (newLineNumber != previousLineNumber)
            {
                this.line.Number = newLineNumber;
                return new List<int> { previousLineNumber, newLineNumber };

            }

            return Enumerable.Empty<int>().ToList();
        }
    }
}
