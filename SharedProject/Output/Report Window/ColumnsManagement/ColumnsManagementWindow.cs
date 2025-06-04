using FineCodeCoverage.Wpf;

namespace FineCodeCoverage.Output
{
#pragma warning disable CA1501
    internal class ColumnsManagementWindow : ResourceDialogWindowBase<ReportColumnsManagementViewModel>
#pragma warning restore CA1501
    {
        private const string ResourcePath = "Output/Report Window/ColumnsManagement/ColumnsManagementDataTemplate.xaml";

        public ColumnsManagementWindow(ReportColumnsManagementViewModel reportColumnsManagmentViewModel) : base(reportColumnsManagmentViewModel, ResourcePath)
        {
            this.Title = "Report Column Management";
            this.SizeToContent = System.Windows.SizeToContent.WidthAndHeight;
        }
    }
}