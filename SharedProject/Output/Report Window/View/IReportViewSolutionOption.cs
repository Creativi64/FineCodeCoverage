using System;

namespace FineCodeCoverage.Output
{
    internal interface IReportViewSolutionOption
    {
        event EventHandler UnloadedEvent;
        ReportViewSolutionOptionValue Value { get; set; }
    }
}