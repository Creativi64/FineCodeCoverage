using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    [Export(typeof(ITrackingLineFactory))]
    internal class TrackingLineFactory : ITrackingLineFactory
    {
        private readonly ILineTracker _lineTracker;

        [ImportingConstructor]
        public TrackingLineFactory(
            ILineTracker lineTracker
        ) => _lineTracker = lineTracker;

        public ITrackingLine Create(
            ITrackingSpan startTrackingSpan,
            ITextSnapshot currentSnapshot,
            DynamicCoverageType dynamicCoverageType)
            => new TrackingLine(startTrackingSpan, currentSnapshot, _lineTracker, dynamicCoverageType);

        public ITrackingLine Create(
           ITrackingSpan startTrackingSpan,
           int originalLineNumber,
           DynamicCoverageType dynamicCoverageType)
            => new TrackingLine(startTrackingSpan, _lineTracker, dynamicCoverageType, originalLineNumber);
    }
}
