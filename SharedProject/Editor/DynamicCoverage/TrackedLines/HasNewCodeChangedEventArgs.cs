using System;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal sealed class HasNewCodeChangedEventArgs : EventArgs
    {
        public HasNewCodeChangedEventArgs(string filePath, bool hasNewCode)
        {
            FilePath = filePath;
            HasNewCode = hasNewCode;
        }

        public string FilePath { get; }

        public bool HasNewCode { get; }
    }
}
