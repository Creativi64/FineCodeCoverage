using FineCodeCoverage.Editor.DynamicCoverage;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.Tagging.Base
{
    internal interface IDynamicLineAndSnapshotSpan
    {
        IDynamicLine Line { get; }
        SnapshotSpan Span { get; }
    }
}
