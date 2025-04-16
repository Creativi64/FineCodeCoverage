using System.Collections.Generic;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal class DirtyTrackingLine : ITrackingLine
    {
        private readonly ITrackingLine trackingLine;
        private readonly IDynamicCoberturaLine dynamicCoberturaLine;

        public DirtyTrackingLine(ITrackingLine trackingLine, IDynamicCoberturaLine dynamicCoberturaLine)
        {
            this.trackingLine = trackingLine;
            this.dynamicCoberturaLine = dynamicCoberturaLine;
        }
        public IDynamicLine Line => this.trackingLine.Line;

        public List<int> GetUpdatedLineNumbers(ITextSnapshot currentSnapshot)
        {
            List<int> updatedLineNumbers = this.trackingLine.GetUpdatedLineNumbers(currentSnapshot);
            if (this.dynamicCoberturaLine != null && updatedLineNumbers.Count > 0)
            {
                this.dynamicCoberturaLine.LineMoved(this.Line.Number);
            }

            return updatedLineNumbers;
        }
    }
}
