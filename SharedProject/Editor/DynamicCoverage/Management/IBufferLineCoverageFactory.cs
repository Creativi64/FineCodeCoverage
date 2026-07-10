using FineCodeCoverage.Utilities.Events;

namespace FineCodeCoverage.Editor.DynamicCoverage.Management
{
    internal interface IBufferLineCoverageFactory
    {
        IBufferLineCoverage Create(
             ITextInfo textInfo, IEventAggregator eventAggregator, ITrackedLinesFactory trackedLinesFactory);
    }
}
