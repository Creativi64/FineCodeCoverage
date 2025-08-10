using FineCodeCoverage.Editor.DynamicCoverage.Common;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.Tagging.Base
{
    internal interface IDynamicLineAndSnapshotSpan
    {
        IDynamicLine Line { get; }

        SnapshotSpan Span { get; }
    }
}
