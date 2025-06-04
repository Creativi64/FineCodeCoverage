using System;
using System.IO;

namespace FineCodeCoverage.Core.Utilities
{
    internal class ComparableFile : IEquatable<ComparableFile>
    {
        private readonly int _hashCode;

        public FileInfo FileInfo { get; }

        public string RelativePath { get; }

        public override int GetHashCode() => this._hashCode;

        public bool Equals(ComparableFile other) => this._hashCode.Equals(other._hashCode);

        public override bool Equals(object obj) => obj is ComparableFile other && this.Equals(other);

        public ComparableFile(FileInfo fileInfo, string relativePath)
        {
            this.FileInfo = fileInfo;
            this.RelativePath = relativePath;
            this._hashCode = string.Format("{0}|{1}|{2}", this.RelativePath, this.FileInfo.Length, this.FileInfo.LastWriteTimeUtc.Ticks).GetHashCode();
        }
    }
}
