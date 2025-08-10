using FineCodeCoverage.Editor.DynamicCoverage.Common;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.Tagging.Base
{
    internal sealed class DynamicLineAndSnapshotSpan : IDynamicLineAndSnapshotSpan
    {
        public DynamicLineAndSnapshotSpan(IDynamicLine line, SnapshotSpan span)
        {
            Line = line;
            Span = span;
        }

        public IDynamicLine Line { get; }

        public SnapshotSpan Span { get; }
    }
}
