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
        private readonly Dictionary<string, List<ICoberturaLine>> coberturaLinesLookup = new Dictionary<string, List<ICoberturaLine>>();
        private readonly IReadOnlyList<IAssembly> assemblies;
        private readonly LineComparer lineComparer = new LineComparer();

        public ReportFileLineCoverage(IReadOnlyList<IAssembly> assemblies)
                => this.assemblies = assemblies;

        public static Dictionary<string, List<ICodeElement>> GetCodeElementLookup(IReadOnlyList<IAssembly> assemblies)
            => assemblies.SelectMany(assembly => assembly.Classes.SelectMany(c => c.FileCodeElements))
                .GroupBy(kvp => kvp.Key).ToDictionary(g => g.Key, g => g.SelectMany(kvp => kvp.Value).ToList());

        private Dictionary<string, List<ICodeElement>> FileLookup
            => this.fileLookup ?? (this.fileLookup = GetCodeElementLookup(this.assemblies));

        public List<ICoberturaLine> GetLines(string filePath)
            {
                if (!this.FileLookup.TryGetValue(filePath, out List<ICodeElement> codeElements))
                {
                    return Enumerable.Empty<ICoberturaLine>().ToList();
                }

                if(this.coberturaLinesLookup.TryGetValue(filePath, out List<ICoberturaLine> coberturaLines))
                {
                    return coberturaLines;
                }

                var lines = codeElements.SelectMany(codeElement => codeElement.Lines).OrderBy(l => l.Number).Distinct(this.lineComparer).ToList();
                this.coberturaLinesLookup.Add(filePath, lines);
                return lines;
            }
        }
}
