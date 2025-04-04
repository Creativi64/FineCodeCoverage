using FineCodeCoverage.Core.Utilities.Solution;
using FineCodeCoverage.Core.Utilities;
using System.ComponentModel.Composition;
using System;

namespace FineCodeCoverage.Output
{
    internal class ReportViewSolutionOptionValue
    {
        public ReportStyle ReportStyle { get; set; }
        public ReportContentType ReportContent { get; set; }

        public string SelectedRepository { get; set; }
        public string SelectedBranchName { get; set; }

    }

    interface IReportViewSolutionOption
    {
        event EventHandler LoadedEvent;
        event EventHandler UnloadedEvent;
        ReportViewSolutionOptionValue Value { get; set; }
    }

    [Export(typeof(ISolutionOption))]
    [Export(typeof(IReportViewSolutionOption))]
    internal class ReportViewSolutionOption : SolutionOption<ReportViewSolutionOptionValue>, IReportViewSolutionOption
    {
        [ImportingConstructor]
        public ReportViewSolutionOption(IJsonConvertService jsonConvertService) : base(jsonConvertService)
        {
        }

        protected override void Loaded(ReportViewSolutionOptionValue previousValue)
        {
            LoadedEvent?.Invoke(this, EventArgs.Empty);
            base.Loaded(previousValue);
        }

        protected override void Saved()
        {
            UnloadedEvent?.Invoke(this, EventArgs.Empty);
            base.Saved();
        }

        protected override ReportViewSolutionOptionValue GetDefaultValue()
        {
            return new ReportViewSolutionOptionValue
            {
                ReportStyle = ReportStyle.Assembly,
                ReportContent = ReportContentType.Full,
            };
        }

        public override string Key { get; protected set; } = "FCC_ReportView";

        public event EventHandler LoadedEvent;
        public event EventHandler UnloadedEvent;
    }
}
