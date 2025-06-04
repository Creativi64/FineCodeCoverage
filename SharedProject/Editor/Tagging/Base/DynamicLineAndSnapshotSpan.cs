using FineCodeCoverage.Editor.DynamicCoverage;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.Tagging.Base
{
    internal class DynamicLineAndSnapshotSpan : IDynamicLineAndSnapshotSpan
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
