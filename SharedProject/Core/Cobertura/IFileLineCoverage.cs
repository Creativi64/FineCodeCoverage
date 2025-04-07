using System.Collections.Generic;

namespace FineCodeCoverage.Engine.Model
{
    internal interface IFileLineCoverage
    {
        void Add(string filePath, IEnumerable<ILine> line);
        IEnumerable<ILine> GetLines(string filePath);
        void Sort();
        void UpdateRenamed(string oldFilePath, string newFilePath);
    }
}
