
using System.Collections.Generic;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    internal class CoverageChangedMessage
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {
        public IBufferLineCoverage BufferLineCoverage { get; }
        public string FilePath { get; }
        public IEnumerable<int> ChangedLineNumbers { get; }

        public CoverageChangedMessage(IBufferLineCoverage bufferLineCoverage, string filePath, IEnumerable<int> changedLineNumbers)
        {
            this.BufferLineCoverage = bufferLineCoverage;
            this.FilePath = filePath;
            this.ChangedLineNumbers = changedLineNumbers;
        }

        public override bool Equals(object obj) => obj is CoverageChangedMessage message &&
                message.BufferLineCoverage == this.BufferLineCoverage &&
                message.FilePath == this.FilePath &&
                message.ChangedLineNumbers == this.ChangedLineNumbers;
    }
}
