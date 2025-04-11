using System;
using System.Collections.Generic;
using System.Linq;
using FineCodeCoverage.Engine.ReportGenerator;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal class ReportFileLineCoverage : IFileLineCoverage
    {
        private Dictionary<string, ISourceFile> sourceFileLookup;
        private readonly Func<IDirectory> directoryProvider;

        private Dictionary<string, ISourceFile> SourceFileLookup
        {
            get
            {
                if (this.sourceFileLookup == null)
                {
                    this.CollectSourceFiles();
                }

                return this.sourceFileLookup;
            }
        }

        public ReportFileLineCoverage(Func<IDirectory> directoryProvider)
            => this.directoryProvider = directoryProvider;

        public List<ICoberturaLine> GetLines(string filePath)
        {
            if (!this.SourceFileLookup.TryGetValue(filePath, out ISourceFile sourceFile))
            {
                return Enumerable.Empty<ICoberturaLine>().ToList();
            }

            IEnumerable<ICodeElement> codeElements = sourceFile.Classes.SelectMany(c => c.CodeElements);
            return codeElements.SelectMany(codeElement => this.GetLines(codeElement)).ToList();
        }

        private CoverageType ConvertLineVisitStatus(LineVisitStatus lineVisitStatus)
        {
            switch (lineVisitStatus)
            {
                case LineVisitStatus.Covered:
                    return CoverageType.Covered;
                case LineVisitStatus.NotCovered:
                    return CoverageType.NotCovered;
                case LineVisitStatus.PartiallyCovered:
                    return CoverageType.Partial;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private List<ICoberturaLine> GetLines(ICodeElement codeElement)
        {
            var coberturaLines = new List<ICoberturaLine>();
            int lineNumber = codeElement.StartLine;
            foreach (LineVisitStatus lineVisitStatus in codeElement.LineVisitStatuses)
            {
                if (lineVisitStatus != LineVisitStatus.NotCoverable)
                {
                    coberturaLines.Add(new CoberturaLine(lineNumber, this.ConvertLineVisitStatus(lineVisitStatus)));
                }

                lineNumber++;
            }

            return coberturaLines;
        }

        private void CollectSourceFiles()
        {
            this.sourceFileLookup = new Dictionary<string, ISourceFile>();
            this.CollectSourceFiles(this.directoryProvider(), this.sourceFileLookup);
        }

        private void CollectSourceFiles(IDirectory directory, Dictionary<string, ISourceFile> sourceFileLookup)
        {
            foreach (ISourceFile sourceFile in directory.SourceFiles)
            {
                sourceFileLookup.Add(sourceFile.Path, sourceFile);
            }

            foreach (IDirectory subDirectory in directory.SubDirectories)
            {
                this.CollectSourceFiles(subDirectory, sourceFileLookup);
            }
        }
    }
}
