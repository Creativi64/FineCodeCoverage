using System;
using System.IO;

namespace FineCodeCoverage.Core.Utilities
{
    internal class ComparableFile : IEquatable<ComparableFile>
    {
        private readonly int hashCode;

        public FileInfo FileInfo { get; }

        public string RelativePath { get; }

        public override int GetHashCode() => this.hashCode;

        public bool Equals(ComparableFile other) => this.hashCode.Equals(other.hashCode);

        public ComparableFile(FileInfo fileInfo, string relativePath)
        {
            this.FileInfo = fileInfo;
            this.RelativePath = relativePath;
            this.hashCode = string.Format("{0}|{1}|{2}", this.RelativePath, this.FileInfo.Length, this.FileInfo.LastWriteTimeUtc.Ticks).GetHashCode();
        }
    }
}