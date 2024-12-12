using Palmmedia.ReportGenerator.Core.Reporting;

namespace FineCodeCoverage.ReportGeneration
{
    internal interface IReportConfigurationFactory
    {
        IReportConfiguration Create(FCCReportConfiguration fccReportConfiguration);
    }
}
