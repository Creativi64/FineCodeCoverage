using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    [ExcludeFromCodeCoverage]
    [Export(typeof(ITrackingSpanRangeFactory))]
    internal sealed class TrackingSpanRangeFactory : ITrackingSpanRangeFactory
    {
        private readonly ILineTracker _lineTracker;

        [ImportingConstructor]
        public TrackingSpanRangeFactory(ILineTracker lineTracker) => _lineTracker = lineTracker;

        public ITrackingSpanRange Create(ITrackingSpan startTrackingSpan, ITrackingSpan endTrackingSpan, ITextSnapshot currentSnapshot)
            => new TrackingSpanRange(startTrackingSpan, endTrackingSpan, currentSnapshot, _lineTracker);
    }
}
