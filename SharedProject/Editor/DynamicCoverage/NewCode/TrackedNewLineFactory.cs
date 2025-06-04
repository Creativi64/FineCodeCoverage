using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    [ExcludeFromCodeCoverage]
    [Export(typeof(ITrackedNewCodeLineFactory))]
    internal class TrackedNewLineFactory : ITrackedNewCodeLineFactory
    {
        private readonly ITrackingSpanFactory _trackingLineFactory;
        private readonly ILineTracker _lineTracker;

        [ImportingConstructor]
        public TrackedNewLineFactory(
            ITrackingSpanFactory trackingLineFactory,
            ILineTracker lineTracker
            )
        {
            this._trackingLineFactory = trackingLineFactory;
            this._lineTracker = lineTracker;
        }
        public ITrackedNewCodeLine Create(
            ITextSnapshot textSnapshot,
            SpanTrackingMode spanTrackingMode,
            int lineNumber
        )
        {
            ITrackingSpan trackingSpan = this._trackingLineFactory.CreateTrackingSpan(
                textSnapshot, lineNumber, spanTrackingMode);
            return new TrackedNewCodeLine(trackingSpan, lineNumber, this._lineTracker);
        }
    }
}