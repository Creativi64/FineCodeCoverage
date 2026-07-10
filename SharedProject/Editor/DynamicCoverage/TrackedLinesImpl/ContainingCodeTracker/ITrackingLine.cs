using System.Collections.Generic;
using FineCodeCoverage.Editor.DynamicCoverage.Common;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal interface ITrackingLine
    {
        IDynamicLine Line { get; }

        List<int> GetUpdatedLineNumbers(ITextSnapshot currentSnapshot);
    }
}
