using System;

namespace FineCodeCoverage.Output
{
    internal enum ReportStyle { Assembly, Source }
    internal enum ReportContentType { Full, Changeset }
    internal interface IReportViews
    {
        event EventHandler Changed;
        ReportStyle ReportStyle { get; }
        ReportContentType ReportContentType { get; }
    }
}
