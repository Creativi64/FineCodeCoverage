using System.Collections.Generic;
using FineCodeCoverage.Editor.DynamicCoverage.Common;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal interface ITrackedCoverageLines
    {
        IEnumerable<IDynamicLine> Lines { get; }

        FirstTrackedCoverageLineInfo GetFirstTrackedCoverageLineInfo();

        IEnumerable<int> GetUpdatedLineNumbers(ITextSnapshot currentSnapshot);
    }
}
