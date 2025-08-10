using System.Collections.Generic;
using FineCodeCoverage.Editor.DynamicCoverage.Common;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal sealed class ContainingCodeTrackerState
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
