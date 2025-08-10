using System.Collections.Generic;
using FineCodeCoverage.Editor.DynamicCoverage.Common;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal interface IUpdatableDynamicLines
    {
        IEnumerable<IDynamicLine> Lines { get; }

        void Deleted();

        IEnumerable<int> GetUpdatedLineNumbers(
            TrackingSpanRangeProcessResult trackingSpanRangeProcessResult,
            ITextSnapshot currentSnapshot,
            List<LineRange> newSpanAndLineRanges);
    }
}
