using System;

namespace FineCodeCoverage.Output
{
    internal interface IReportViews
    {
        event EventHandler<ReportViewChangedEventArgs> Changed;

        ReportStyle ReportStyle { get; }

        IChangeset GetChangeset();
    }
}
