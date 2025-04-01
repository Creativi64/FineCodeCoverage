using FineCodeCoverage.Core.Utilities.Solution;
using System;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Output
{
    [Export(typeof(IReportViews))]
    [Export(typeof(IReportViewSelectorModel))]
    internal class ReportViews : IReportViews, IReportViewSelectorModel
    {
        private readonly ReportViewSolutionOption reportViewSolutionOption;

        [ImportingConstructor]
        public ReportViews(ReportViewSolutionOption reportViewSolutionOption)
        {
            reportViewSolutionOption.LoadedEvent += ReportViewSolutionOption_LoadedEvent;
            this.reportViewSolutionOption = reportViewSolutionOption;
#if VS2022
            CanUseChangeset = true;
#endif
        }

        private void ReportViewSolutionOption_LoadedEvent(object sender, SolutionOptionLoadEventArgs<ReportViewSolutionOptionValue> e)
        {
            var previous = e.PreviousValue;
            var reportStyleDidChange = previous.ReportStyle != ReportStyle;
            var reportContentDidChange = previous.ReportContent != ReportContent;
            if (reportStyleDidChange && reportContentDidChange)
            {
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        public InitialReportViewState Initialize()
        {
            return new InitialReportViewState(ReportStyle);
        }

        public void Update(ReportStyle reportStyle)
        {
            ReportStyle = reportStyle;
            reportViewSolutionOption.Value = new ReportViewSolutionOptionValue
            {
                ReportStyle = ReportStyle,
                ReportContent = ReportContent,
            };
            Changed?.Invoke(this, EventArgs.Empty);
        }

        public ReportStyle ReportStyle { get; private set; }
        public ReportContent ReportContent { get; }
        public bool CanUseChangeset { get; }

        public event EventHandler Changed;
    }

}
