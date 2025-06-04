using System.Collections.Generic;
using System.Linq;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Editor.DynamicCoverage.Utilities;
using FineCodeCoverage.Engine.ReportGenerator;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal class ReportFileLineCoverage : IFileLineCoverage
    {
        private Dictionary<string, List<ICodeElement>> _fileLookup;
        private readonly Dictionary<string, FileLines> _fileLinesLookup = new Dictionary<string, FileLines>();
        private readonly IReadOnlyList<IAssembly> _assemblies;
        private readonly IDateTimeService _dateTimeService;

        private class LineComparer : IEqualityComparer<ICoberturaLine>
        {
            public bool Equals(ICoberturaLine x, ICoberturaLine y) => x.Number == y.Number;

            public int GetHashCode(ICoberturaLine obj) => obj.Number;
        }

        private readonly LineComparer _lineComparer = new LineComparer();

        public ReportFileLineCoverage(IReadOnlyList<IAssembly> assemblies, IDateTimeService dateTimeService)
        {
            this._assemblies = assemblies;
            this._dateTimeService = dateTimeService;
        }

        public static Dictionary<string, List<ICodeElement>> GetCodeElementLookup(IReadOnlyList<IAssembly> assemblies)
            => assemblies.SelectMany(assembly => assembly.Classes.SelectMany(c => c.FileCodeElements))
                .GroupBy(kvp => kvp.Key).ToDictionary(g => g.Key, g => g.SelectMany(kvp => kvp.Value).ToList());

        private Dictionary<string, List<ICodeElement>> FileLookup
            => this._fileLookup ?? (this._fileLookup = GetCodeElementLookup(this._assemblies));

        public IFileLines GetLines(string filePath)
        {
            if (this._fileLinesLookup.TryGetValue(filePath, out FileLines fileLines))
            {
                return fileLines;
            }

            if (!this.FileLookup.TryGetValue(filePath, out List<ICodeElement> codeElements))
            {
                return null;
            }

            var lines = codeElements.SelectMany(codeElement => codeElement.Lines).OrderBy(l => l.Number).Distinct(this._lineComparer).ToList();
            fileLines = new FileLines(lines, this._dateTimeService);
            this._fileLinesLookup.Add(filePath, fileLines);
            _ = this.FileLookup.Remove(filePath);
            return fileLines;
        }

        public void OutOfDate(string filePath)
        {
            _ = this.FileLookup.Remove(filePath);
            _ = this._fileLinesLookup.Remove(filePath);
        }

        internal void FilesRenamed(IReadOnlyList<FileRename> fileRenames)
        {
            _ = fileRenames.TryUpdateDictionary(this._fileLinesLookup);
            _ = fileRenames.TryUpdateDictionary(this.FileLookup);
        }
    }
}