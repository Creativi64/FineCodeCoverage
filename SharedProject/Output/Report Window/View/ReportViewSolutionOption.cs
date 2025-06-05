using System.ComponentModel.Composition;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Core.Utilities.Solution;

namespace FineCodeCoverage.Output
{
    [Export(typeof(ISolutionOption))]
    [Export(typeof(IReportViewSolutionOption))]
    internal class ReportViewSolutionOption : SolutionOption<ReportViewSolutionOptionValue>, IReportViewSolutionOption
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
