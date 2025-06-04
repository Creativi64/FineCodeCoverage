namespace FineCodeCoverage.Output
{
    internal class ReportContentTypeViewModel
    {
        public ReportContentTypeViewModel(ReportContentType reportContentType, string display)
        {
            this.ReportContentType = reportContentType;
            this.Display = display;
        }

        public ReportContentType ReportContentType { get; }
        public string Display { get; }
        public override string ToString() => this.Display;
    }
}
