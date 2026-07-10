using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using FineCodeCoverage.Editor.DynamicCoverage.Common;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage.NotIncluded
{
    [ExcludeFromCodeCoverage]
    [Export(typeof(INotIncludedLineFactory))]
    internal sealed class NotIncludedLineFactory : INotIncludedLineFactory
    {
        private readonly ITrackingLineFactory _trackingLineFactory;

        [ImportingConstructor]
        public NotIncludedLineFactory(
             ITrackingLineFactory trackingLineFactory) => _trackingLineFactory = trackingLineFactory;

        public ITrackingLine Create(ITrackingSpan startTrackingSpan, ITextSnapshot currentSnapshot)
            => _trackingLineFactory.Create(startTrackingSpan, currentSnapshot, DynamicCoverageType.NotIncluded);
    }
}
