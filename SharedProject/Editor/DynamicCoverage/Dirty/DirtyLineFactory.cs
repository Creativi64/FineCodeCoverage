using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using FineCodeCoverage.Collection.ReportGeneration;
using FineCodeCoverage.Editor.DynamicCoverage.Common;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage.Dirty
{
    [ExcludeFromCodeCoverage]
    [Export(typeof(IDirtyLineFactory))]
    internal sealed class DirtyLineFactory : IDirtyLineFactory
    {
        private readonly ITrackingLineFactory _trackingLineFactory;

        [ImportingConstructor]
        public DirtyLineFactory(ITrackingLineFactory trackingLineFactory)
            => _trackingLineFactory = trackingLineFactory;

        public ITrackingLine Create(
            ITrackingSpan trackingSpan,
            int originalLineNumber,
            IDynamicCoberturaLine dynamicCoberturaLine)
            => new DirtyTrackingLine(
                _trackingLineFactory.Create(trackingSpan, originalLineNumber, DynamicCoverageType.Dirty),
                dynamicCoberturaLine);
    }
}
