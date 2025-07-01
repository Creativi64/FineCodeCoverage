using FineCodeCoverage.Collection.ReportGeneration;

namespace FineCodeCoverage.Collection.Messages
{
    public sealed class ReportFilesMessage
    {
        public IReportResult ReportResult { get; set; }

        public string CoberturaFile { get; set; }
    }
}
