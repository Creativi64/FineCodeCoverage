using FineCodeCoverage.Utilities.Events;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal interface IBufferLineCoverageFactory
    {
        IBufferLineCoverage Create(
             ITextInfo textInfo, IEventAggregator eventAggregator, ITrackedLinesFactory trackedLinesFactory);
    }
}
