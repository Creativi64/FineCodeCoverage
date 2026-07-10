using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage.NotIncluded
{
    internal interface INotIncludedLineFactory
    {
        ITrackingLine Create(ITrackingSpan startTrackingSpan, ITextSnapshot currentSnapshot);
    }
}
