using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal interface INewCodeTracker
    {
        event EventHandler<HasNewCodeChangedEventArgs> HasNewCodeChanged;

        IEnumerable<IDynamicLine> Lines { get; }

        IEnumerable<int> GetChangedLineNumbers(ITextSnapshot currentSnapshot, List<CodeSpanRange> codeSpanRanges);

        IEnumerable<int> GetChangedLineNumbers(ITextSnapshot currentSnapshot, List<LineRange> ranges);
    }
}
