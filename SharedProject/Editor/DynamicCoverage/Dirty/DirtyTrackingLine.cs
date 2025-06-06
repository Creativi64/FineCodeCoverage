using System.Collections.Generic;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal sealed class DirtyTrackingLine : ITrackingLine
    {
        private readonly ITrackingLine _trackingLine;
        private readonly IDynamicCoberturaLine _dynamicCoberturaLine;

        public DirtyTrackingLine(ITrackingLine trackingLine, IDynamicCoberturaLine dynamicCoberturaLine)
        {
            _trackingLine = trackingLine;
            _dynamicCoberturaLine = dynamicCoberturaLine;
        }

        public IDynamicLine Line => _trackingLine.Line;

        public List<int> GetUpdatedLineNumbers(ITextSnapshot currentSnapshot)
        {
            List<int> updatedLineNumbers = _trackingLine.GetUpdatedLineNumbers(currentSnapshot);
            if (_dynamicCoberturaLine != null && updatedLineNumbers.Count > 0)
            {
                _dynamicCoberturaLine.LineMoved(Line.LineNumber);
            }

            return updatedLineNumbers;
        }
    }
}
