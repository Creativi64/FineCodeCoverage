using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage.Common
{
    internal interface ITrackingLineFactory
    {
        ITrackingLine Create(
            ITrackingSpan startTrackingSpan,
            ITextSnapshot currentSnapshot,
            DynamicCoverageType dynamicCoverageType);

        ITrackingLine Create(
            ITrackingSpan startTrackingSpan,
            int originalLineNumber,
            DynamicCoverageType dynamicCoverageType);
    }
}
