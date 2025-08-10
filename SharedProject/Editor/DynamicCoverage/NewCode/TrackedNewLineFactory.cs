using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage.NewCode
{
    [ExcludeFromCodeCoverage]
    [Export(typeof(ITrackedNewCodeLineFactory))]
    internal sealed class TrackedNewLineFactory : ITrackedNewCodeLineFactory
    {
        private readonly ITrackingSpanFactory _trackingLineFactory;
        private readonly ILineTracker _lineTracker;

        [ImportingConstructor]
        public TrackedNewLineFactory(
            ITrackingSpanFactory trackingLineFactory,
            ILineTracker lineTracker)
        {
            _trackingLineFactory = trackingLineFactory;
            _lineTracker = lineTracker;
        }

        public ITrackedNewCodeLine Create(
            ITextSnapshot textSnapshot,
            SpanTrackingMode spanTrackingMode,
            int lineNumber)
        {
            ITrackingSpan trackingSpan = _trackingLineFactory.CreateTrackingSpan(
                textSnapshot, lineNumber, spanTrackingMode);
            return new TrackedNewCodeLine(trackingSpan, lineNumber, _lineTracker);
        }
    }
}
