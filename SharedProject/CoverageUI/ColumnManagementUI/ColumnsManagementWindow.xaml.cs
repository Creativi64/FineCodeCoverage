using VsThemedDialogs;

namespace FineCodeCoverage.Output
{
    /// <summary>
    /// report columns management window.
    /// </summary>
    internal sealed partial class ColumnsManagementWindow : ThemedDialogWindow
    {
        public ColumnsManagementWindow2(ReportColumnsManagementViewModel reportColumnsManagmentViewModel)
        {
            this.DataContext = reportColumnsManagmentViewModel;
            InitializeComponent();
        }
    }
}
