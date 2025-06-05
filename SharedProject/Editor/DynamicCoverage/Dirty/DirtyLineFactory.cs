using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    [ExcludeFromCodeCoverage]
    [Export(typeof(IDirtyLineFactory))]
    internal class DirtyLineFactory : IDirtyLineFactory
    {
        private readonly ITrackingLineFactory _trackingLineFactory;

        [ImportingConstructor]
        public DirtyLineFactory(ITrackingLineFactory trackingLineFactory)
            => _trackingLineFactory = trackingLineFactory;

        public ITrackingLine Create(
            ITrackingSpan trackingSpan,
            int originalLineNumber,
            IDynamicCoberturaLine dynamicCoberturaLine) => new DirtyTrackingLine(
        ) => new DirtyTrackingLine(
                _trackingLineFactory.Create(trackingSpan, originalLineNumber, DynamicCoverageType.Dirty),
                dynamicCoberturaLine);
    }
}
