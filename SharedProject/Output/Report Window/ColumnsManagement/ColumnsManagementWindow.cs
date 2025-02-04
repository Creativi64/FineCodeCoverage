using FineCodeCoverage.Wpf;

namespace FineCodeCoverage.Output
{
    internal class ColumnsManagementWindow : ResourceDialogWindowBase<ReportColumnsManagementViewModel>
    {
        private const string resourcePath = "Output/Report Window/ColumnsManagement/ColumnsManagementDataTemplate.xaml";
        public ColumnsManagementWindow(ReportColumnsManagementViewModel reportColumnsManagmentViewModel) : base(reportColumnsManagmentViewModel, resourcePath)
        {
            this.Title = "Report Column Management";
            this.SizeToContent = System.Windows.SizeToContent.Height;
        }
    }
}
