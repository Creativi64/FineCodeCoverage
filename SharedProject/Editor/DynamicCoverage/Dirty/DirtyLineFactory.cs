using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    [ExcludeFromCodeCoverage]
    [Export(typeof(IDirtyLineFactory))]
    internal class DirtyLineFactory : IDirtyLineFactory
    {
        private readonly ITrackingLineFactory trackingLineFactory;

        [ImportingConstructor]
        public DirtyLineFactory(ITrackingLineFactory trackingLineFactory) => this.trackingLineFactory = trackingLineFactory;
        public ITrackingLine Create(ITrackingSpan trackingSpan, ITextSnapshot snapshot, IDynamicCoberturaLine dynamicCoberturaLine)
            => new DirtyTrackingLine(
                    this.trackingLineFactory.Create(trackingSpan, snapshot, DynamicCoverageType.Dirty),
                    dynamicCoberturaLine);
    }
}
