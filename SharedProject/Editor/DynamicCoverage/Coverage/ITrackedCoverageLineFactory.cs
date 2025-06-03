using FineCodeCoverage.Engine.ReportGenerator;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal interface ITrackedCoverageLineFactory
    {
        ITrackedCoverageLine Create(ITrackingSpan trackingSpan, ICoberturaLine line);
    }
}