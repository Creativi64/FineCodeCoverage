using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    [ExcludeFromCodeCoverage]
    [Export(typeof(INotIncludedLineFactory))]
    internal class NotIncludedLineFactory : INotIncludedLineFactory
    {
        private readonly ITrackingLineFactory trackingLineFactory;

        [ImportingConstructor]
        public NotIncludedLineFactory(
             ITrackingLineFactory lineTracker
        ) => this.trackingLineFactory = lineTracker;

        public ITrackingLine Create(ITrackingSpan startTrackingSpan, ITextSnapshot currentSnapshot)
            => this.trackingLineFactory.Create(startTrackingSpan, currentSnapshot, DynamicCoverageType.NotIncluded);
    }
}