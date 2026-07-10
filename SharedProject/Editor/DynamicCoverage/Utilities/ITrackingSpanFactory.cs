using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal interface ITrackingSpanFactory
    {
        ITrackingSpan CreateTrackingSpan(ITextSnapshot textSnapshot, int lineNumber, SpanTrackingMode spanTrackingMode);
    }
}
