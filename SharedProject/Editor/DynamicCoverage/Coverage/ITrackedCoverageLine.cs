using System.Collections.Generic;
using FineCodeCoverage.Collection.ReportGeneration;
using FineCodeCoverage.Editor.DynamicCoverage.Common;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage.Coverage
{
    internal interface ITrackedCoverageLine
    {
        List<int> GetUpdateLineNumbers(ITextSnapshot currentSnapshot);

        IDynamicLine Line { get; }

        IDynamicCoberturaLine DynamicCoberturaLine { get; }
    }
}
