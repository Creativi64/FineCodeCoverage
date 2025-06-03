namespace FineCodeCoverage.Output
{
    internal class ReportViewChangedEventArgs
    {
        public ReportViewChangedEventArgs(bool changeSetChanged) => this.ChangesetChanged = changeSetChanged;

        public bool ChangesetChanged { get; }
    }
}