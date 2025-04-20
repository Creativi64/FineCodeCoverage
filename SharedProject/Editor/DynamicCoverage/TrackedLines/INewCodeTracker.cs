using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    class HasNewCodeChangedEventArgs : EventArgs
    {
        public HasNewCodeChangedEventArgs(string filePath, bool hasNewCode)
        {
            this.FilePath = filePath;
            this.HasNewCode = hasNewCode;
        }

        public string FilePath { get; }
        public bool HasNewCode { get; }
    }

    internal interface INewCodeTracker
    {
        event EventHandler<HasNewCodeChangedEventArgs> HasNewCodeChanged;
        IEnumerable<IDynamicLine> Lines { get; }

        IEnumerable<int> GetChangedLineNumbers(ITextSnapshot currentSnapshot, List<CodeSpanRange> codeSpanRanges);

        IEnumerable<int> GetChangedLineNumbers(ITextSnapshot currentSnapshot, List<LineRange> ranges);
    }
}
