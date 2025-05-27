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
        private readonly Dictionary<string, bool> sourceFilesPathsWithNewCode = new Dictionary<string, bool>();
        private List<SourceFile> sourceFiles;
        private IDirectory directory;

        public Report(NewReportMessage message)
        {
            TestAssemblyNames = message.CoverageProjects?.Select(cp => cp.ProjectName).ToList();
            Assemblies = message.Report.Assemblies;
            MetricTypes = message.Report.MetricTypes;
            (message.Report as IDynamicReportResult).FileRenamedEvent += Report_FileRenamedEvent;
        }

        private void Report_FileRenamedEvent(object sender, IReadOnlyList<FileRename> fileRenames)
        {
            // only dealing with directories - ReportGeneratorUtil dealing with Assemblies => Class.FileCodeElements
            fileRenames.TryUpdateDictionary(sourceFilesPathsWithNewCode);
            if (Directory != null)
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
                ClearDirectory();
                DirectoryStructureChanged?.Invoke(this, EventArgs.Empty);
                return true;
            }

            return false;
        }

        private void UpdateSourceFiles(List<FileRename> fileRenames)
        {
            foreach (var sourceFile in sourceFiles)
            {
                foreach (var fileRename in fileRenames)
                {
                    if (sourceFile.Path == fileRename.OldFilePath)
                    {
                        sourceFile.Path = fileRename.NewFilePath;
                        fileRenames.Remove(fileRename);
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

        public IDirectory Directory
        {
            get
            {
                return directory ?? (directory = CreateDirectory());
            }
        }

        private void ClearDirectory()
        {
            sourceFiles = null;
            directory = null;
        }

        private List<SourceFile> GetSourceFiles()
        {
            return this.Assemblies.SelectMany(a =>
            {
                return a.Classes.SelectMany(
                    pc => pc.FileCodeElements.Select(
                        kvp => new { SourcePath = kvp.Key, ClassName = pc.DisplayName, CodeElements = kvp.Value }));
            }).GroupBy(a => a.SourcePath)
            .Select(g =>
            {
                var sourceFileClasses = g.Select(a => new SourceFileClass(a.ClassName, a.SourcePath, a.CodeElements)).ToList();
                var path = g.Key;
                var hasNewCode = sourceFilesPathsWithNewCode.ContainsKey(path);
                return new SourceFile(path, sourceFileClasses, hasNewCode);
            }).ToList();
        }

        private List<SourceFile> SourceFiles
        {
            get
            {
                return sourceFiles ?? (sourceFiles = GetSourceFiles());
            }
        }

        private IDirectory CreateDirectory()
        {
            return CreateDirectory(SourceFiles);
        }

        private IDirectory CreateDirectory(IEnumerable<ISourceFile> sourceFiles)
        {
            return DirectoryResultsTreeBuilder.BuildDirectoryTree(sourceFiles.ToList());
        }

        public void NewCodeChanged(string path, bool hasNewCode)
        {
            if (hasNewCode)
            {
                sourceFilesPathsWithNewCode.Add(path, true);
            }
            else
            {
                _ = sourceFilesPathsWithNewCode.Remove(path);
            }

            foreach (var sourceFile in SourceFiles)
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
