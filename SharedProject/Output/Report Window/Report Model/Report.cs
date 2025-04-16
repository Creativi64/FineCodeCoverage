using FineCodeCoverage.Engine.ReportGenerator;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FineCodeCoverage.Output
{
    internal class Report
    {
        public Report(NewReportMessage message)
        {
            TestAssemblyNames = message.CoverageProjects?.Select(cp => cp.ProjectName).ToList();
            Assemblies = message.Report.Assemblies;
            MetricTypes = message.Report.MetricTypes;
        }

        public List<string> TestAssemblyNames { get; }
        public IReadOnlyList<IAssembly> Assemblies { get; }
        public IReadOnlyList<MetricType> MetricTypes { get; internal set; }
        private IDirectory directory;
        public IDirectory Directory
        {
            get
            {
                return directory ?? (directory = CreateDirectory());
            }
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
                return new SourceFile(g.Key, sourceFileClasses);
            }).ToList();
        }
        private List<SourceFile> sourceFiles;
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

        internal void NewCodeChanged(string path, bool hasNewCode)
        {
            foreach (var sourceFile in SourceFiles)
            {
                if(sourceFile.Path == path)
                {
                    sourceFile.SetHasNewCode(hasNewCode);
                    break;
                }
            }
        }
    }

}
