using System.Collections.Generic;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal interface ITrackedCoverageLines
    {
        IEnumerable<IDynamicLine> Lines { get; }

        IDynamicCoberturaLine GetStartDynamicCoberturaLine();
        IEnumerable<int> GetUpdatedLineNumbers(ITextSnapshot currentSnapshot);
    }
}
