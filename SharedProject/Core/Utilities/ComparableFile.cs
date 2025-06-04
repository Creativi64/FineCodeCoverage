using System;
using System.IO;

namespace FineCodeCoverage.Core.Utilities
{
    internal class ComparableFile : IEquatable<ComparableFile>
    {
        private readonly int _hashCode;

        public FileInfo FileInfo { get; }

        public string RelativePath { get; }

        public override int GetHashCode() => _hashCode;

        public bool Equals(ComparableFile other) => _hashCode.Equals(other._hashCode);

        public override bool Equals(object obj) => obj is ComparableFile other && Equals(other);

        public ComparableFile(FileInfo fileInfo, string relativePath)
        {
            FileInfo = fileInfo;
            RelativePath = relativePath;
            _hashCode = string.Format("{0}|{1}|{2}", RelativePath, FileInfo.Length, FileInfo.LastWriteTimeUtc.Ticks).GetHashCode();
        }
    }
}
