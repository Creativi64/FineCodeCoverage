using FineCodeCoverage.Wpf;

namespace FineCodeCoverage.Output
{
    internal partial class ReportViewSelectorWindow : BaseDialogWindowTemplated
    {
        public ReportViewSelectorWindow(ReportViewSelectorViewModel reportViewSelectorViewModel)
            : base(reportViewSelectorViewModel) => InitializeComponent();
    }
}
