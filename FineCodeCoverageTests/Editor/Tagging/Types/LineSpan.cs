using FineCodeCoverage.Editor.DynamicCoverage.Common;
using FineCodeCoverage.Editor.Tagging.Base;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverageTests.Editor.Tagging.Types
{
    internal class LineSpan : IDynamicLineAndSnapshotSpan
    {
        public IDynamicLine Line { get; set; }

        public SnapshotSpan Span { get; set; }
    }
}