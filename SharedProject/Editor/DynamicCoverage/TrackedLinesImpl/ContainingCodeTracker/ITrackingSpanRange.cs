using System.Collections.Generic;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal interface ITrackingSpanRange
    {
        TrackingSpanRangeProcessResult Process(ITextSnapshot currentSnapshot, List<LineRange> newSpanAndLineRanges);
        ITrackingSpan GetFirstTrackingSpan();
        CodeSpanRange ToCodeSpanRange();
    }
}
