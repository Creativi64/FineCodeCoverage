using System.ComponentModel.Composition;

namespace FineCodeCoverage.Output
{
    [Export(typeof(IReportViewService))]
    internal class ReportViewService : IReportViewService
    {
        private readonly IReportViewSelectorModel _reportViewSelectorModel;

        [ImportingConstructor]
        public ReportViewService(
            IReportViewSelectorModel reportViewSelectorModel) => _reportViewSelectorModel = reportViewSelectorModel;

        public void Show()
        {
            // could import the view model if it has no state
            var vm = new ReportViewSelectorViewModel(_reportViewSelectorModel);
            _ = new ReportViewSelectorWindow(vm).ShowModal();
        }
    }
}
