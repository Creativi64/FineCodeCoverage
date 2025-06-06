namespace FineCodeCoverage.Output
{
    internal sealed class ReportViewChangedEventArgs
    {
        public ReportViewChangedEventArgs(bool changeSetChanged) => ChangesetChanged = changeSetChanged;

        public bool ChangesetChanged { get; }
    }
}
