using FineCodeCoverage.Collection.ReportGeneration;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage.Dirty
{
    internal interface IDirtyLineFactory
    {
        ITrackingLine Create(
            ITrackingSpan trackingSpan, int originalLineNumber, IDynamicCoberturaLine dynamicCoberturaLine);
    }
}
