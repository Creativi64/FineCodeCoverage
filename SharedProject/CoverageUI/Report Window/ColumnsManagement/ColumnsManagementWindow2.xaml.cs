using FineCodeCoverage.Wpf;

namespace FineCodeCoverage.Output
{
    /// <summary>
    /// report columns management window.
    /// </summary>
    internal sealed partial class ColumnsManagementWindow2 : BaseDialogWindowTemplated
    {
        public ColumnsManagementWindow2(ReportColumnsManagementViewModel reportColumnsManagmentViewModel)
            : base(reportColumnsManagmentViewModel) => InitializeComponent();
    }
}
