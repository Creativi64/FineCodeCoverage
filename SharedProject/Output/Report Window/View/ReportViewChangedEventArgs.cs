namespace FineCodeCoverage.Output
{
    internal class ReportViewChangedEventArgs
    {
        public ReportViewChangedEventArgs(bool changeSetChanged)
        {
            ChangesetChanged = changeSetChanged;
        }

        public bool ChangesetChanged { get; }
    }
}
