using System.ComponentModel.Composition;
using FineCodeCoverage.Engine;

namespace FineCodeCoverage.Output
{
    [Export(typeof(IReportColumnsService))]
    class ReportColumnsService : IReportColumnsService
    {
        private readonly IReportColumnManager reportColumnsManager;
        private readonly IMessageBox messageBox;

        [ImportingConstructor]
        public ReportColumnsService(IReportColumnManager reportColumnsManager, IMessageBox messageBox)
        {
            this.reportColumnsManager = reportColumnsManager;
            this.messageBox = messageBox;
        }
        public void ManageColumns()
        {
            var vm = new ReportColumnsManagementViewModel(this.reportColumnsManager, this.messageBox);
            var columnsManagementWindow = new ColumnsManagementWindow2(vm);
            _ = columnsManagementWindow.ShowModal();
        }
    }
}
