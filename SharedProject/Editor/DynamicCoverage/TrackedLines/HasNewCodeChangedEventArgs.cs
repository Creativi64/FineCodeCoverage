using System;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal class HasNewCodeChangedEventArgs : EventArgs
    {
        public HasNewCodeChangedEventArgs(string filePath, bool hasNewCode)
        {
            this.FilePath = filePath;
            this.HasNewCode = hasNewCode;
        }

        public string FilePath { get; }
        public bool HasNewCode { get; }
    }
}
