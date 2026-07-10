using System.Collections.Generic;
using FineCodeCoverage.Editor.DynamicCoverage.Management;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.Tagging.Base
{
    internal interface IDynamicLineAndSnapshotSpansLogic
    {
        List<IDynamicLineAndSnapshotSpan> Apply(
            IBufferLineCoverage bufferLineCoverage, NormalizedSnapshotSpanCollection spans);
    }
}
