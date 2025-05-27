using System;

namespace FineCodeCoverage.Output
{
    internal enum ReportStyle { Assembly, Source }
    internal enum ReportContentType { Full, Changeset }

    internal class ReportViewChangedEventArgs
    {
        public ReportViewChangedEventArgs(bool changeSetChanged)
        {
            ChangesetChanged = changeSetChanged;
        }

        public bool ChangesetChanged { get; }
    }


    internal interface IReportViews
    {
        event EventHandler<ReportViewChangedEventArgs> Changed;
        ReportStyle ReportStyle { get; }
        IChangeset GetChangeset();
    }
}
