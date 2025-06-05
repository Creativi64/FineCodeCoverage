using FineCodeCoverage.Engine.ReportGenerator;

namespace FineCodeCoverage.Engine
{
    internal class ReportFilesMessage
    {
        public IReportResult ReportResult { get; set; }

        public string CoberturaFile { get; set; }
    }
}
