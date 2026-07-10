using System.Collections.Generic;

namespace FineCodeCoverage.Editor.DynamicCoverage.Coverage
{
    internal interface ITrackedCoverageLinesFactory
    {
        ITrackedCoverageLines Create(List<ITrackedCoverageLine> trackedCoverageLines);
    }
}
