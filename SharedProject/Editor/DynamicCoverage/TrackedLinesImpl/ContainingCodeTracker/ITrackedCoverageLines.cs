using System.Collections.Generic;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal class FirstTrackedCoverageLineInfo
    {
        public FirstTrackedCoverageLineInfo(int originalLineNumber, IDynamicCoberturaLine dynamicCoberturaLine)
        {
            this.OriginalLineNumber = originalLineNumber;
            this.DynamicCoberturaLine = dynamicCoberturaLine;
        }

        public int OriginalLineNumber { get; }
        public IDynamicCoberturaLine DynamicCoberturaLine { get; }
    }
    internal interface ITrackedCoverageLines
    {
        IEnumerable<IDynamicLine> Lines { get; }

        FirstTrackedCoverageLineInfo GetFirstTrackedCoverageLineInfo();
        IEnumerable<int> GetUpdatedLineNumbers(ITextSnapshot currentSnapshot);
    }
}
