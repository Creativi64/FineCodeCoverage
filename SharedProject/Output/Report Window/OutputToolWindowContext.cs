using FineCodeCoverage.Options;

namespace FineCodeCoverage.Output
{
    internal class OutputToolWindowContext
    {
        public ReportViewModel ReportViewModel { get; set; }
        public IAppOptionsProvider AppOptionsProvider { get; set; }
        public bool ShowToolWindowToolbar() => this.AppOptionsProvider.Get().ShowToolWindowToolbar;
    }
}
