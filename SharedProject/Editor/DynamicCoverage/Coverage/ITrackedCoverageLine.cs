using System.Collections.Generic;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal interface ITrackedCoverageLine
    {
        List<int> GetUpdateLineNumbers(ITextSnapshot currentSnapshot);
        IDynamicLine Line { get; }
        IDynamicCoberturaLine DynamicCoberturaLine { get; }
    }
}
