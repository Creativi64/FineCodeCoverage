using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    [Export(typeof(ITrackingLineFactory))]
    internal class TrackingLineFactory : ITrackingLineFactory
    {
        private readonly ILineTracker lineTracker;

        [ImportingConstructor]
        public TrackingLineFactory(
            ILineTracker lineTracker
        ) => this.lineTracker = lineTracker;

        public ITrackingLine Create(
            ITrackingSpan startTrackingSpan,
            ITextSnapshot currentSnapshot,
            DynamicCoverageType dynamicCoverageType)
            => new TrackingLine(startTrackingSpan, currentSnapshot, this.lineTracker, dynamicCoverageType);

        public ITrackingLine Create(
            ITrackingSpan startTrackingSpan,
           int originalLineNumber,
           DynamicCoverageType dynamicCoverageType) => new TrackingLine(startTrackingSpan, this.lineTracker, dynamicCoverageType, originalLineNumber);
    }
}
