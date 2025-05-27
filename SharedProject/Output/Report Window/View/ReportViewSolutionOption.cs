using System;
using System.ComponentModel.Composition;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Core.Utilities.Solution;

namespace FineCodeCoverage.Output
{
    internal class ReportViewSolutionOptionValue
    {
        public static ReportViewSolutionOptionValue Default => new ReportViewSolutionOptionValue { ReportStyle = ReportStyle.Assembly, ReportContent = ReportContentType.Full };
        public ReportStyle ReportStyle { get; set; }
        public ReportContentType ReportContent { get; set; }

        public string SelectedRepository { get; set; }
        public string SelectedBranchName { get; set; }

    }

    interface IReportViewSolutionOption
    {
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

        protected override ReportViewSolutionOptionValue GetDefaultValue()
        {
            return ReportViewSolutionOptionValue.Default;
        }

        public override string Key { get; protected set; } = "FCC_ReportView";
    }
}
