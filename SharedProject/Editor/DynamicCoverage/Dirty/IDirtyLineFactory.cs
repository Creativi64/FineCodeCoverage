using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal interface IDirtyLineFactory
    {
        ITrackingLine Create(
            ITrackingSpan trackingSpan, int originalLineNumber, IDynamicCoberturaLine dynamicCoberturaLine);
    }
}
