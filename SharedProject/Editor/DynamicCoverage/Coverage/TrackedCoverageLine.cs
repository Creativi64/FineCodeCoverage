using System;
using System.Collections.Generic;
using System.Linq;
using FineCodeCoverage.Engine.ReportGenerator;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal class TrackedCoverageLine : ITrackedCoverageLine
    {
        private readonly ITrackingSpan _trackingSpan;
        private readonly ILineTracker _lineTracker;
        private readonly DynamicLine _line;
        private readonly Action<int> _updateDynamicCoberturaLine = (_) => { };

        public IDynamicLine Line => this._line;

        public IDynamicCoberturaLine DynamicCoberturaLine { get; }

        public TrackedCoverageLine(ITrackingSpan trackingSpan, ICoberturaLine coberturaLine, ILineTracker lineTracker)
        {
            this._line = DynamicLine.FromCoberturaLine(coberturaLine);
            this._trackingSpan = trackingSpan;
            this._lineTracker = lineTracker;
            if (coberturaLine is IDynamicCoberturaLine dynamicCoberturaLine)
            {
                this.DynamicCoberturaLine = dynamicCoberturaLine;
                this._updateDynamicCoberturaLine = (newLineNumber) => dynamicCoberturaLine.LineMoved(newLineNumber);
            }
        }

        public List<int> GetUpdateLineNumbers(ITextSnapshot currentSnapshot)
        {
            int previousLineNumber = this.Line.LineNumber;
            int newLineNumber = this._lineTracker.GetLineNumber(this._trackingSpan, currentSnapshot, true);
            if (newLineNumber != previousLineNumber)
            {
                this._line.LineNumber = newLineNumber;
                this._updateDynamicCoberturaLine(newLineNumber);
                return new List<int> { previousLineNumber, newLineNumber };

            }

            return Enumerable.Empty<int>().ToList();
        }
    }
}