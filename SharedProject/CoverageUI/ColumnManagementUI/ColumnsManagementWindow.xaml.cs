using VsThemedDialogs;

namespace FineCodeCoverage.Output
{
    /// <summary>
    /// report columns management window.
    /// </summary>
    internal sealed partial class ColumnsManagementWindow : ThemedDialogWindow
    {
        public ColumnsManagementWindow(ReportColumnsManagementViewModel reportColumnsManagmentViewModel)
        {
            DataContext = reportColumnsManagmentViewModel;
            InitializeComponent();
        }
    }
}
