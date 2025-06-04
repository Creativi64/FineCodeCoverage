using System.Collections.Generic;
using FineCodeCoverage.Engine.ReportGenerator;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal interface ICodeSpanRangeContainingCodeTrackerFactory
    {
        IContainingCodeTracker CreateNotIncluded(ITextSnapshot textSnapshot, CodeSpanRange containingRange, SpanTrackingMode spanTrackingMode);
        IContainingCodeTracker CreateCoverageLines(ITextSnapshot textSnapshot, List<ICoberturaLine> coberturaLines, CodeSpanRange containingRange, SpanTrackingMode spanTrackingMode);
        IContainingCodeTracker CreateOtherLines(ITextSnapshot textSnapshot, CodeSpanRange containingRange, SpanTrackingMode spanTrackingMode);
    }
}
