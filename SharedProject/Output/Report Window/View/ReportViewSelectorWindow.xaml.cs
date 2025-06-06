using FineCodeCoverage.Wpf;

namespace FineCodeCoverage.Output
{
    /// <summary>
    /// report view selector window.
    /// </summary>
    internal partial class ReportViewSelectorWindow : BaseDialogWindowTemplated
    {
        public ReportViewSelectorWindow(ReportViewSelectorViewModel reportViewSelectorViewModel)
            : base(reportViewSelectorViewModel) => InitializeComponent();
    }
}
