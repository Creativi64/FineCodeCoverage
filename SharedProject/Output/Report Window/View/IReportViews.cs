using System;

namespace FineCodeCoverage.Output
{
    internal enum ReportStyle { Assembly, Source }
    internal enum ReportContent { Full, Changeset }
    internal interface IReportViews
    {
        event EventHandler Changed;
        ReportStyle ReportStyle { get; }
        ReportContent ReportContent { get; }
    }
}
