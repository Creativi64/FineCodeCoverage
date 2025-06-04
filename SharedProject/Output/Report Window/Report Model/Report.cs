using System;
using System.Collections.Generic;
using System.Linq;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine.ReportGenerator;

namespace FineCodeCoverage.Output
{
    internal class Report
    {
        public event EventHandler<EventArgs> DirectoryStructureChanged;
        private readonly Dictionary<string, bool> _sourceFilesPathsWithNewCode = new Dictionary<string, bool>();
        private List<SourceFile> _sourceFiles;
        private IDirectory _directory;

        public Report(NewReportMessage message)
        {
            this.TestAssemblyNames = message.CoverageProjects?.Select(cp => cp.ProjectName).ToList();
            this.Assemblies = message.Report.Assemblies;
            this.MetricTypes = message.Report.MetricTypes;
            (message.Report as IDynamicReportResult).FileRenamedEvent += this.Report_FileRenamedEvent;
        }

        private void Report_FileRenamedEvent(object sender, IReadOnlyList<FileRename> fileRenames)
        {
            // only dealing with directories - ReportGeneratorUtil dealing with Assemblies => Class.FileCodeElements
            _ = fileRenames.TryUpdateDictionary(this._sourceFilesPathsWithNewCode);
            if (this.Directory != null)
            {
                if (!this.HasDirectoryStructureChanged(fileRenames))
                {
                    this.UpdateSourceFiles(fileRenames.ToList());
                }
            }
        }

        private bool HasDirectoryStructureChanged(IReadOnlyList<FileRename> fileRenames)
        {
            if (fileRenames.Any(fileRename => fileRename.HasDirectoryChanged()))
            {
                this.ClearDirectory();
                DirectoryStructureChanged?.Invoke(this, EventArgs.Empty);
                return true;
            }

            return false;
        }

        private void UpdateSourceFiles(List<FileRename> fileRenames)
        {
            foreach (SourceFile sourceFile in this._sourceFiles)
            {
                foreach (FileRename fileRename in fileRenames)
                {
                    if (sourceFile.Path == fileRename.OldFilePath)
                    {
                        sourceFile.Path = fileRename.NewFilePath;
                        _ = fileRenames.Remove(fileRename);
                        break;
                    }
                }

                if (fileRenames.Count == 0)
                {
                    break;
                }
            }
        }

        public List<string> TestAssemblyNames { get; }

        public IReadOnlyList<IAssembly> Assemblies { get; }

        public IReadOnlyList<MetricType> MetricTypes { get; }

        public IDirectory Directory => this._directory ?? (this._directory = this.CreateDirectory());

        private void ClearDirectory()
        {
            this._sourceFiles = null;
            this._directory = null;
        }

        private List<SourceFile> GetSourceFiles()
            => this.Assemblies.SelectMany(a => a.Classes.SelectMany(
                    pc => pc.FileCodeElements.Select(
                        kvp => new { SourcePath = kvp.Key, ClassName = pc.DisplayName, CodeElements = kvp.Value }
                    )
                )
            ).GroupBy(a => a.SourcePath)
            .Select(g =>
            {
                var sourceFileClasses = g.Select(a => new SourceFileClass(a.ClassName, a.SourcePath, a.CodeElements)).ToList();
                string path = g.Key;
                bool hasNewCode = this._sourceFilesPathsWithNewCode.ContainsKey(path);
                return new SourceFile(path, sourceFileClasses, hasNewCode);
            }).ToList();

        private List<SourceFile> SourceFiles => this._sourceFiles ?? (this._sourceFiles = this.GetSourceFiles());

        private IDirectory CreateDirectory() => CreateDirectory(this.SourceFiles);

        private static IDirectory CreateDirectory(IEnumerable<ISourceFile> sourceFiles)
            => DirectoryResultsTreeBuilder.BuildDirectoryTree(sourceFiles.ToList());

        public void NewCodeChanged(string path, bool hasNewCode)
        {
            if (hasNewCode)
            {
                this._sourceFilesPathsWithNewCode.Add(path, true);
            }
            else
            {
                _ = this._sourceFilesPathsWithNewCode.Remove(path);
            }

            foreach (SourceFile sourceFile in this.SourceFiles)
            {
                if (sourceFile.Path == path)
                {
                    sourceFile.SetHasNewCode(hasNewCode);
                    break;
                }
            }
        }
    }
}