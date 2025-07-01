using System.ComponentModel.Composition;
using FineCodeCoverage.Utilities.Wrappers;

namespace FineCodeCoverage.Output
{
    [Export(typeof(IReportColumnsService))]
    internal sealed class ReportColumnsService : IReportColumnsService
    {
        private readonly IReportColumnManager _reportColumnsManager;
        private readonly IMessageBox _messageBox;

        [ImportingConstructor]
        public ReportColumnsService(IReportColumnManager reportColumnsManager, IMessageBox messageBox)
        {
            _reportColumnsManager = reportColumnsManager;
            _messageBox = messageBox;
        }

        public void ManageColumns()
        {
            var vm = new ReportColumnsManagementViewModel(_reportColumnsManager, _messageBox);
            var columnsManagementWindow = new ColumnsManagementWindow2(vm);
            _ = columnsManagementWindow.ShowModal();
        }
    }
}
