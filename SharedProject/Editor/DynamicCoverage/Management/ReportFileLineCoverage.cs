using System.Collections.Generic;
using System.Linq;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Editor.DynamicCoverage.Utilities;
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
        private readonly IDateTimeService dateTimeService;
        private readonly LineComparer lineComparer = new LineComparer();

        public ReportFileLineCoverage(IReadOnlyList<IAssembly> assemblies,IDateTimeService dateTimeService)
        {
            this.assemblies = assemblies;
            this.dateTimeService = dateTimeService;
        }

        public static Dictionary<string, List<ICodeElement>> GetCodeElementLookup(IReadOnlyList<IAssembly> assemblies)
            => assemblies.SelectMany(assembly => assembly.Classes.SelectMany(c => c.FileCodeElements))
                .GroupBy(kvp => kvp.Key).ToDictionary(g => g.Key, g => g.SelectMany(kvp => kvp.Value).ToList());

        private Dictionary<string, List<ICodeElement>> FileLookup
            => this.fileLookup ?? (this.fileLookup = GetCodeElementLookup(this.assemblies));

        public IFileRenameListener FileRenameListener { get; }

        public IFileLines GetLines(string filePath)
        {
            if (this.fileLinesLookup.TryGetValue(filePath, out FileLines fileLines))
            {
                return fileLines;
            }

            if (!this.FileLookup.TryGetValue(filePath, out List<ICodeElement> codeElements))
            {
                return null;
            }

            var lines = codeElements.SelectMany(codeElement => codeElement.Lines).OrderBy(l => l.Number).Distinct(this.lineComparer).ToList();
            fileLines = new FileLines(lines, this.dateTimeService);
            this.fileLinesLookup.Add(filePath, fileLines);
            _ = this.FileLookup.Remove(filePath);
            return fileLines;
        }

        public void OutOfDate(string filePath)
        {
            _ = this.FileLookup.Remove(filePath);
            _ = this.fileLinesLookup.Remove(filePath);
        }

        private static void TryUpdate<T>(List<FileRename> fileRenames, IDictionary<string, T> fileDictionary)
            => fileRenames.ForEach(fileRename =>
                {
                    if (fileDictionary != null && fileDictionary.TryGetValue(fileRename.OldFilePath, out T fileLines))
                    {
                        _ = fileDictionary.Remove(fileRename.OldFilePath);
                        fileDictionary.Add(fileRename.NewFilePath, fileLines);
                    }
                });

        internal void FilesRenamed(List<FileRename> fileRenames)
        {
            TryUpdate(fileRenames, this.fileLinesLookup);
            TryUpdate(fileRenames, this.FileLookup);
        }
    }
}
