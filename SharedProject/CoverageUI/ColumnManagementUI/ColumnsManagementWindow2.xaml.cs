using FineCodeCoverage.Wpf;

namespace FineCodeCoverage.Output
{
    /// <summary>
    /// report columns management window.
    /// </summary>
    internal sealed partial class ColumnsManagementWindow2 : ThemedDialogWindow
    {
        public ColumnsManagementWindow2(ReportColumnsManagementViewModel reportColumnsManagmentViewModel)
        {
            this.DataContext = reportColumnsManagmentViewModel;
            InitializeComponent();
        }
    }
}
