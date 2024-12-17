using FineCodeCoverage.Options;

namespace FineCodeCoverage.Output
{
    internal class ReportToolWindowContext
    {
        public ReportViewModel ReportViewModel { get; set; }
        public IAppOptionsProvider AppOptionsProvider { get; set; }
        public bool ShowToolWindowToolbar() => this.AppOptionsProvider.Get().ShowToolWindowToolbar;
    }
}
