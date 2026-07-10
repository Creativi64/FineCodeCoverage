using System.Collections.Generic;
using FineCodeCoverage.Editor.DynamicCoverage.Common;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal interface IContainingCodeTracker
    {
        IContainingCodeTrackerProcessResult ProcessChanges(ITextSnapshot currentSnapshot, List<LineRange> newSpanAndLineRanges);

        ContainingCodeTrackerState GetState();

        void Deleted();

        IEnumerable<IDynamicLine> Lines { get; }
    }
}
