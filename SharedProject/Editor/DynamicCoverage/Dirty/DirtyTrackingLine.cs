using System.Collections.Generic;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal class DirtyTrackingLine : ITrackingLine
    {
        private readonly ITrackingLine _trackingLine;
        private readonly IDynamicCoberturaLine _dynamicCoberturaLine;

        public DirtyTrackingLine(ITrackingLine trackingLine, IDynamicCoberturaLine dynamicCoberturaLine)
        {
            this._trackingLine = trackingLine;
            this._dynamicCoberturaLine = dynamicCoberturaLine;
        }
        public IDynamicLine Line => this._trackingLine.Line;

        public List<int> GetUpdatedLineNumbers(ITextSnapshot currentSnapshot)
        {
            List<int> updatedLineNumbers = this._trackingLine.GetUpdatedLineNumbers(currentSnapshot);
            if (this._dynamicCoberturaLine != null && updatedLineNumbers.Count > 0)
            {
                this._dynamicCoberturaLine.LineMoved(this.Line.LineNumber);
            }

            return updatedLineNumbers;
        }
    }
}