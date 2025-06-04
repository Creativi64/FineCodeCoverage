using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using FineCodeCoverage.Engine.ReportGenerator;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    [ExcludeFromCodeCoverage]
    [Export(typeof(ITrackedCoverageLineFactory))]
    internal class TrackedCoverageLineFactory : ITrackedCoverageLineFactory
    {
        private readonly ILineTracker _lineTracker;

        [ImportingConstructor]
        public TrackedCoverageLineFactory(ILineTracker lineTracker) => this._lineTracker = lineTracker;
        public ITrackedCoverageLine Create(ITrackingSpan trackingSpan, ICoberturaLine line)
            => new TrackedCoverageLine(trackingSpan, line, this._lineTracker);
    }
}