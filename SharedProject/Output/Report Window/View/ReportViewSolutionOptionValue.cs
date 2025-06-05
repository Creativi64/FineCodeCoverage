namespace FineCodeCoverage.Output
{
    internal class ReportViewSolutionOptionValue
    {
        public static ReportViewSolutionOptionValue Default => new ReportViewSolutionOptionValue
        {
            ReportStyle = ReportStyle.Assembly,
            ReportContent = ReportContentType.Full,
        };

        public ReportStyle ReportStyle { get; set; }

        public ReportContentType ReportContent { get; set; }

        public string SelectedRepository { get; set; }

        public string SelectedBranchName { get; set; }
    }
}
