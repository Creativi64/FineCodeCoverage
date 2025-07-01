using System.ComponentModel.Composition;
using FineCodeCoverage.Core.Utilities.Solution;
using FineCodeCoverage.Utilities.Wrappers;

namespace FineCodeCoverage.Output
{
    [Export(typeof(ISolutionOption))]
    [Export(typeof(IReportViewSolutionOption))]
    internal sealed class ReportViewSolutionOption : SolutionOption<ReportViewSolutionOptionValue>, IReportViewSolutionOption
    {
        [ImportingConstructor]
        public ReportViewSolutionOption(IJsonConvertService jsonConvertService)
            : base(jsonConvertService)
        {
        }

        protected override ReportViewSolutionOptionValue GetDefaultValue() => ReportViewSolutionOptionValue.Default;

        public override string Key { get; protected set; } = "FCC_ReportView";
    }
}
