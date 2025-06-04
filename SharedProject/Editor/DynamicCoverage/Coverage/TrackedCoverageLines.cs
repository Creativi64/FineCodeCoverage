using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal class TrackedCoverageLines : ITrackedCoverageLines
    {
        private readonly List<ITrackedCoverageLine> _trackedCoverageLines;

        public IEnumerable<IDynamicLine> Lines
            => this._trackedCoverageLines.Select(coverageLine => coverageLine.Line);

        public TrackedCoverageLines(List<ITrackedCoverageLine> coverageLines)
            => this._trackedCoverageLines = coverageLines;

        public IEnumerable<int> GetUpdatedLineNumbers(ITextSnapshot currentSnapshot)
            => this._trackedCoverageLines.SelectMany(
                trackedCoverageLine => trackedCoverageLine.GetUpdateLineNumbers(currentSnapshot)
            );

        public FirstTrackedCoverageLineInfo GetFirstTrackedCoverageLineInfo()
        {
            ITrackedCoverageLine first = this._trackedCoverageLines[0];
            return new FirstTrackedCoverageLineInfo(first.Line.OriginalLineNumber, first.DynamicCoberturaLine);
        }
    }
}