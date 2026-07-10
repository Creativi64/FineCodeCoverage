using VsThemedDialogs;

namespace FineCodeCoverage.Output
{
    /// <summary>
    /// report view selector window.
    /// </summary>
    internal sealed partial class ReportViewSelectorWindow : ThemedDialogWindow
    {
        public ReportViewSelectorWindow(ReportViewSelectorViewModel reportViewSelectorViewModel)
        {
            this.DataContext = reportViewSelectorViewModel;
            InitializeComponent();
        }
    }
}
