using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    class NewCodeChangedEventArgs : EventArgs
    {
        public NewCodeChangedEventArgs(string filePath, bool hasNewCode)
        {
            this.FilePath = filePath;
            this.HasNewCode = hasNewCode;
        }

        public string FilePath { get; }
        public bool HasNewCode { get; }
    }
    internal interface INewCodeTracker
    {
        event EventHandler<NewCodeChangedEventArgs> NewCodeChanged;
        IEnumerable<IDynamicLine> Lines { get; }

        IEnumerable<int> GetChangedLineNumbers(
            ITextSnapshot currentSnapshot,
            List<SpanAndLineRange> newSpanChanges,
            IEnumerable<CodeSpanRange> newCodeCodeRanges);
    }
}
