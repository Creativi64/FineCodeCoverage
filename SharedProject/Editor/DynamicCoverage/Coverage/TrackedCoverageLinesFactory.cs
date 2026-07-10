using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;

namespace FineCodeCoverage.Editor.DynamicCoverage.Coverage
{
    [ExcludeFromCodeCoverage]
    [Export(typeof(ITrackedCoverageLinesFactory))]
    internal sealed class TrackedCoverageLinesFactory : ITrackedCoverageLinesFactory
    {
        public ITrackedCoverageLines Create(List<ITrackedCoverageLine> trackedCoverageLines)
            => new TrackedCoverageLines(trackedCoverageLines);
    }
}
