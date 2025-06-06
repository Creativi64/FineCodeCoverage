using FineCodeCoverage.Engine.ReportGenerator;

namespace FineCodeCoverage.Engine
{
    internal sealed class ReportFilesMessage
    {
        public IReportResult ReportResult { get; set; }

        public string CoberturaFile { get; set; }
    }
}
