using System;
using System.Collections.Generic;
using System.Linq;

namespace FineCodeCoverage.Engine.Model
{
    internal class UniqueCoberturaLines : HashSet<ICoberturaLine>
    {
        public UniqueCoberturaLines() : base(new LineComparer())
        {
        }

        public void AddRange(IEnumerable<ICoberturaLine> coberturaLines)
        {
            foreach (var coberturaLine in coberturaLines)
                Add(coberturaLine);
        }

        private IEnumerable<ICoberturaLine> sortedLines;
        public IEnumerable<ICoberturaLine> SortedLines => sortedLines;

        public void Sort()
        {
            sortedLines = this.OrderBy(l => l.Number).ToList();
        }

        class LineComparer : IEqualityComparer<ICoberturaLine>
        {
            public bool Equals(ICoberturaLine x, ICoberturaLine y)
            {
                return x.Number == y.Number;
            }

            public int GetHashCode(ICoberturaLine obj)
            {
                return obj.Number;
            }
        }
    }

    // FileLineCoverage maps from a filename to the list of lines in the file
    internal class FileLineCoverage : IFileLineCoverage
    {
        private readonly Dictionary<string, UniqueCoberturaLines> m_coverageLines = new Dictionary<string, UniqueCoberturaLines>(StringComparer.OrdinalIgnoreCase);

        public void Add(string filePath, IEnumerable<ICoberturaLine> lines)
        {
            if (!m_coverageLines.TryGetValue(filePath, out var fileCoverageLines))
            {
                fileCoverageLines = new UniqueCoberturaLines();
                m_coverageLines.Add(filePath, fileCoverageLines);
            }

            fileCoverageLines.AddRange(lines);
        }

        public void Sort()
        {
            foreach (var lines in m_coverageLines.Values)
                lines.Sort();
        }

        public IEnumerable<ICoberturaLine> GetLines(string filePath)
        {
            if (!m_coverageLines.TryGetValue(filePath, out var lines))
            {
                return Enumerable.Empty<ICoberturaLine>().ToList();
            }
            return lines.SortedLines;
        }

        public void UpdateRenamed(string oldFilePath, string newFilePath)
        {
            if(m_coverageLines.TryGetValue(oldFilePath, out var lines))
            {
                m_coverageLines.Add(newFilePath, lines);
                m_coverageLines.Remove(oldFilePath);
            }
        }
    }
}
