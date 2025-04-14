using System.Collections.Generic;
using System.Linq;
using FineCodeCoverage.Engine.ReportGenerator;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    class LineComparer : IEqualityComparer<ICoberturaLine>
    {
        public bool Equals(ICoberturaLine x, ICoberturaLine y) => x.Number == y.Number;

        public int GetHashCode(ICoberturaLine obj) => obj.Number;
    }

    internal class ReportFileLineCoverage : IFileLineCoverage
    {
        private Dictionary<string, List<ICodeElement>> fileLookup;
        private readonly Dictionary<string, FileLines> fileLinesLookup = new Dictionary<string, FileLines>();
        private readonly IReadOnlyList<IAssembly> assemblies;
        private readonly LineComparer lineComparer = new LineComparer();

        public ReportFileLineCoverage(IReadOnlyList<IAssembly> assemblies)
                => this.assemblies = assemblies;

        public static Dictionary<string, List<ICodeElement>> GetCodeElementLookup(IReadOnlyList<IAssembly> assemblies)
            => assemblies.SelectMany(assembly => assembly.Classes.SelectMany(c => c.FileCodeElements))
                .GroupBy(kvp => kvp.Key).ToDictionary(g => g.Key, g => g.SelectMany(kvp => kvp.Value).ToList());

        private Dictionary<string, List<ICodeElement>> FileLookup
            => this.fileLookup ?? (this.fileLookup = GetCodeElementLookup(this.assemblies));

        public FileLines GetLines(string filePath)
        {
            if (!this.FileLookup.TryGetValue(filePath, out List<ICodeElement> codeElements))
            {
                return null;
            }

            if(this.fileLinesLookup.TryGetValue(filePath, out FileLines fileLines))
            {
                return fileLines;
            }

            var lines = codeElements.SelectMany(codeElement => codeElement.Lines).OrderBy(l => l.Number).Distinct(this.lineComparer).ToList();
            fileLines = new FileLines(lines);
            this.fileLinesLookup.Add(filePath, fileLines);
            return fileLines;
        }
    }
}
