using System.ComponentModel.Composition;
using FineCodeCoverage.Engine;

namespace FineCodeCoverage.Output
{
    [Export(typeof(IReportColumnsService))]
    class ReportColumnsService : IReportColumnsService
    {
        private readonly IReportColumnManager _reportColumnsManager;
        private readonly IMessageBox _messageBox;

        [ImportingConstructor]
        public ReportColumnsService(IReportColumnManager reportColumnsManager, IMessageBox messageBox)
        {
            this._reportColumnsManager = reportColumnsManager;
            this._messageBox = messageBox;
        }
        public void ManageColumns()
        {
            var vm = new ReportColumnsManagementViewModel(this._reportColumnsManager, this._messageBox);
            var columnsManagementWindow = new ColumnsManagementWindow2(vm);
            _ = columnsManagementWindow.ShowModal();
        }
    }
}
