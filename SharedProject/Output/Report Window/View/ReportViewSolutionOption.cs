using FineCodeCoverage.Core.Utilities.Solution;
using FineCodeCoverage.Core.Utilities;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Output
{
    internal class ReportViewSolutionOptionValue
    {
        public ReportStyle ReportStyle { get; set; }
        public ReportContent ReportContent { get; set; }

        // todo branch and repo

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
                ReportContent = ReportContent.Full,
            };
        }

        public override string Key { get; protected set; } = "FCC_ReportView";
    }
}
