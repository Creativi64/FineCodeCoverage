using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal interface ITrackingLineFactory
    {
        ITrackingLine Create(ITrackingSpan startTrackingSpan,
            ITextSnapshot currentSnapshot,
            DynamicCoverageType dynamicCoverageType);
    }
}
