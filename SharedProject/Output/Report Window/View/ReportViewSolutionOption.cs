using FineCodeCoverage.Core.Utilities.Solution;
using FineCodeCoverage.Core.Utilities;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Output
{
    internal class ReportViewSolutionOptionValue
    {
        public ReportStyle ReportStyle { get; set; }
        public ReportContentType ReportContent { get; set; }

        public string SelectedRepository { get; set; }
        public string SelectedBranchName { get; set; }

    }

    [Export(typeof(ISolutionOption))]
    [Export(typeof(ReportViewSolutionOption))]
    internal class ReportViewSolutionOption : SolutionOption<ReportViewSolutionOptionValue>
    {
        [ImportingConstructor]
        public ReportViewSolutionOption(IJsonConvertService jsonConvertService) : base(jsonConvertService)
        {
            Value = new ReportViewSolutionOptionValue
            {
                ReportStyle = ReportStyle.Assembly,
                ReportContent = ReportContentType.Full,
            };
        }

        public override string Key { get; protected set; } = "FCC_ReportView";
    }
}
