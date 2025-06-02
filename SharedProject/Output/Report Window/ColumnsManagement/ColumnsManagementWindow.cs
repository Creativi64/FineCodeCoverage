using FineCodeCoverage.Wpf;

namespace FineCodeCoverage.Output
{
#pragma warning disable CA1501
    internal class ColumnsManagementWindow : ResourceDialogWindowBase<ReportColumnsManagementViewModel>
#pragma warning restore CA1501
    {
        private const string resourcePath = "Output/Report Window/ColumnsManagement/ColumnsManagementDataTemplate.xaml";
        public ColumnsManagementWindow(ReportColumnsManagementViewModel reportColumnsManagmentViewModel) : base(reportColumnsManagmentViewModel, resourcePath)
        {
            this.Title = "Report Column Management";
            this.SizeToContent = System.Windows.SizeToContent.WidthAndHeight;
        }
    }
}
