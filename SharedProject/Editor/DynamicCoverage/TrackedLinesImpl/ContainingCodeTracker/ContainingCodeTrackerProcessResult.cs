using System.Collections.Generic;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal class ContainingCodeTrackerProcessResult : IContainingCodeTrackerProcessResult
    {
        public ContainingCodeTrackerProcessResult(
            IEnumerable<int> changedLines,
            List<LineRange> unprocessedSpans,
            bool isEmpty
        )
        {
            this.ChangedLines = changedLines;
            this.UnprocessedSpans = unprocessedSpans;
            this.IsEmpty = isEmpty;
        }

        public bool IsEmpty { get; }

        public IEnumerable<int> ChangedLines { get; set; }

        public List<LineRange> UnprocessedSpans { get; }
    }
}
