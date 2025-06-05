using System.Collections.Generic;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    internal class CoverageChangedMessage
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {
        public string FilePath { get; }
        public IEnumerable<int> ChangedLineNumbers { get; }

        public CoverageChangedMessage(string filePath, IEnumerable<int> changedLineNumbers)
        {
            FilePath = filePath;
            ChangedLineNumbers = changedLineNumbers;
        }

        public override bool Equals(object obj) => obj is CoverageChangedMessage message &&
                message.FilePath == FilePath &&
                message.ChangedLineNumbers == ChangedLineNumbers;
    }
}
