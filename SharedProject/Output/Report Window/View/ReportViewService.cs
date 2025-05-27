using System.ComponentModel.Composition;

namespace FineCodeCoverage.Output
{
    [Export(typeof(IReportViewService))]
    internal class ReportViewService : IReportViewService
    {
        private readonly IReportViewSelectorModel reportViewSelectorModel;

        [ImportingConstructor]
        public ReportViewService(
            IReportViewSelectorModel reportViewSelectorModel)
        {
            this.reportViewSelectorModel = reportViewSelectorModel;
        }
        public void Show()
        {
            // could import the view model if it has no state
            var vm = new ReportViewSelectorViewModel(reportViewSelectorModel);
            new ReportViewSelectorWindow(vm).ShowModal();
        }
    }
}
