using System.Collections.Generic;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal class ContainingCodeTrackerState
    {
        public ContainingCodeTrackerState(
            CodeSpanRange codeSpanRange,
            IEnumerable<IDynamicLine> lines)
        {
            CodeSpanRange = codeSpanRange;
            Lines = lines;
        }

        public CodeSpanRange CodeSpanRange { get; }

        public IEnumerable<IDynamicLine> Lines { get; }
    }
}
