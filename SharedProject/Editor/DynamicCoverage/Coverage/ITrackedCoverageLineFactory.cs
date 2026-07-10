using FineCodeCoverage.Collection.ReportGeneration;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage.Coverage
{
    internal interface ITrackedCoverageLineFactory
    {
        ITrackedCoverageLine Create(ITrackingSpan trackingSpan, ICoberturaLine line);
    }
}
