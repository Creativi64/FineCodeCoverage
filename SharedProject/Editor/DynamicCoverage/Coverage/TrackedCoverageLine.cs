using System;
using System.Collections.Generic;
using System.Linq;
using FineCodeCoverage.Collection.ReportGeneration;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal sealed class TrackedCoverageLine : ITrackedCoverageLine
    {
        private readonly ITrackingSpan _trackingSpan;
        private readonly ILineTracker _lineTracker;
        private readonly DynamicLine _line;
        private readonly Action<int> _updateDynamicCoberturaLine = (_) => { };

        public IDynamicLine Line => _line;

        public IDynamicCoberturaLine DynamicCoberturaLine { get; }

        public TrackedCoverageLine(ITrackingSpan trackingSpan, ICoberturaLine coberturaLine, ILineTracker lineTracker)
        {
            _line = DynamicLine.FromCoberturaLine(coberturaLine);
            _trackingSpan = trackingSpan;
            _lineTracker = lineTracker;
            if (!(coberturaLine is IDynamicCoberturaLine dynamicCoberturaLine))
            {
                return;
            }

            DynamicCoberturaLine = dynamicCoberturaLine;
            _updateDynamicCoberturaLine = (newLineNumber) => dynamicCoberturaLine.LineMoved(newLineNumber);
        }

        public List<int> GetUpdateLineNumbers(ITextSnapshot currentSnapshot)
        {
            int previousLineNumber = Line.LineNumber;
            int newLineNumber = _lineTracker.GetLineNumber(_trackingSpan, currentSnapshot, true);
            if (newLineNumber != previousLineNumber)
            {
                _line.LineNumber = newLineNumber;
                _updateDynamicCoberturaLine(newLineNumber);
                return new List<int> { previousLineNumber, newLineNumber };
            }

            return Enumerable.Empty<int>().ToList();
        }
    }
}
