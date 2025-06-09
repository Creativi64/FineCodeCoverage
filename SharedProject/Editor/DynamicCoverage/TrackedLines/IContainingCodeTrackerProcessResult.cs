using System.Collections.Generic;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal interface IContainingCodeTrackerProcessResult
    {
        bool IsEmpty { get; }

        IEnumerable<int> ChangedLines { get; }

        List<LineRange> UnprocessedSpans { get; }
    }
}
