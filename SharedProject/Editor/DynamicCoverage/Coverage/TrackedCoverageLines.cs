using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal class TrackedCoverageLines : ITrackedCoverageLines
    {
        private readonly List<ITrackedCoverageLine> trackedCoverageLines;

        public IEnumerable<IDynamicLine> Lines => this.trackedCoverageLines.Select(coverageLine => coverageLine.Line);
        public TrackedCoverageLines(List<ITrackedCoverageLine> coverageLines) => this.trackedCoverageLines = coverageLines;

        public IEnumerable<int> GetUpdatedLineNumbers(ITextSnapshot currentSnapshot)
            => this.trackedCoverageLines.SelectMany(trackedCoverageLine => trackedCoverageLine.GetUpdateLineNumbers(currentSnapshot));
        public IDynamicCoberturaLine GetStartDynamicCoberturaLine() => this.trackedCoverageLines[0].DynamicCoberturaLine;
    }
}
