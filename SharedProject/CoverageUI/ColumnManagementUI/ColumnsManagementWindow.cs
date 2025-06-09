using FineCodeCoverage.Wpf;

namespace FineCodeCoverage.Output
{
#pragma warning disable CA1501
    internal sealed class ColumnsManagementWindow : ResourceDialogWindowBase<ReportColumnsManagementViewModel>
#pragma warning restore CA1501
    {
        private const string ResourcePath = "CoverageUI/ColumnManagementUI/ColumnsManagement/ColumnsManagementDataTemplate.xaml";

        public ColumnsManagementWindow(ReportColumnsManagementViewModel reportColumnsManagmentViewModel)
            : base(reportColumnsManagmentViewModel, ResourcePath)
        {
            Title = "Report Column Management";
            SizeToContent = System.Windows.SizeToContent.WidthAndHeight;
        }
    }
}
