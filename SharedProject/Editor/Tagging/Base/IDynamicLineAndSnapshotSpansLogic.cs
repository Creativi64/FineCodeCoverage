using System.Collections.Generic;
using FineCodeCoverage.Editor.DynamicCoverage;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.Tagging.Base
{
    internal interface IDynamicLineAndSnapshotSpansLogic
    {
        IEnumerable<IDynamicLineAndSnapshotSpan> Apply(
            IBufferLineCoverage bufferLineCoverage, NormalizedSnapshotSpanCollection spans);
    }
}
