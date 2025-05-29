using System;
using System.Collections.Generic;
using System.Linq;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Editor.DynamicCoverage;

namespace FineCodeCoverage.Engine.ReportGenerator
{
    internal class DynamicReportResult : IDynamicReportResult
    {
        public event EventHandler<IReadOnlyList<FileRename>> FileRenamedEvent;

        public class DynamicCoberturaLine : IDynamicCoberturaLine
        {
            public DynamicCoberturaLine(ICoberturaLine coberturaLine, IDynamicCodeElement codeElement)
            {
                this.Number = coberturaLine.Number;
                this.OriginalLineNumber = this.Number;
                this.CoverageType = coberturaLine.CoverageType;
                this.CodeElement = codeElement;
            }
            public int OriginalLineNumber { get; }
            public int Number { get; private set; }
            public CoverageType CoverageType { get; }
            public IDynamicCodeElement CodeElement { get; }

            public void LineMoved(int newLineNumber) => this.Number = newLineNumber;
        }
        public class DynamicCodeElement : IDynamicCodeElement
        {
            public CodeElementType CodeElementType => this.codeElement.CodeElementType;
            public string Name => this.codeElement.Name;
            public int StartLine => this.codeElement.StartLine;
            public string Path { get; set; }
            public IReadOnlyList<ICoberturaLine> Lines { get; }
            public int BlocksCovered => this.codeElement.BlocksCovered;
            public int BlocksNotCovered => this.codeElement.BlocksNotCovered;
            public int CyclomaticComplexity => this.codeElement.CyclomaticComplexity;
            public int NPathComplexity => this.codeElement.NPathComplexity;
            public int TotalBranches => this.codeElement.TotalBranches;
            public int BranchesCovered => this.codeElement.BranchesCovered;
            public decimal CrapScore => this.codeElement.CrapScore;
            private readonly ICodeElement codeElement;

            public DynamicCodeElement(ICodeElement codeElement)
            {
                this.codeElement = codeElement;
                this.Path = codeElement.Path;
                this.Lines = codeElement.Lines.Select(l => new DynamicCoberturaLine(l, this)).ToList();
            }

            public DynamicCodeElementState State { get; set; }
            public void IsDirty() => this.State = DynamicCodeElementState.Dirty;

            public void Deleted() => this.State = DynamicCodeElementState.Deleted;
        }
        public class DynamicClass : IClass
        {
            public DynamicClass(IClass clss)
            {
                this.DisplayName = clss.DisplayName;
                this.fileCodeElements = clss.FileCodeElements.ToDictionary(kvp => kvp.Key, kvp => (IReadOnlyList<DynamicCodeElement>)kvp.Value.Select(ce => new DynamicCodeElement(ce)).ToList());
                this.SetFileCodeElements();
                this.CodeElements = this.fileCodeElements.Values.SelectMany(ces => ces).ToList();
            }

            private void SetFileCodeElements() => this.FileCodeElements = this.fileCodeElements.ToDictionary(
                   kvp => kvp.Key,
                   kvp => (IReadOnlyList<ICodeElement>)kvp.Value.Cast<ICodeElement>().ToList()
               );

            public string DisplayName { get; }

            private readonly Dictionary<string, IReadOnlyList<DynamicCodeElement>> fileCodeElements;
            public IReadOnlyDictionary<string, IReadOnlyList<ICodeElement>> FileCodeElements { get; private set; }
            public IReadOnlyList<ICodeElement> CodeElements { get; }

            internal void FileRenamed(IReadOnlyList<FileRename> fileRenames)
            {
                fileRenames = fileRenames.TryUpdateDictionary(this.fileCodeElements);
                if (fileRenames.Count > 0)
                {
                    this.SetFileCodeElements();
                }

                foreach (FileRename fileRename in fileRenames)
                {
                    IReadOnlyList<DynamicCodeElement> codeElements = this.fileCodeElements[fileRename.NewFilePath];
                    foreach (DynamicCodeElement dynamicCodeElement in codeElements)
                    {
                        dynamicCodeElement.Path = fileRename.NewFilePath;
                    }
                }
            }
        }
        public class DynamicAssembly : IAssembly
        {
            public DynamicAssembly(IAssembly assembly)
            {
                this.Name = assembly.Name;
                this.ShortName = assembly.ShortName;
                this.Classes = assembly.Classes.Select(cls => new DynamicClass(cls)).ToList();
            }
            public void FileRenamed(IReadOnlyList<FileRename> fileRenames)
            {
                foreach (IClass cls in this.Classes)
                {
                    (cls as DynamicClass).FileRenamed(fileRenames);
                }
            }
            public string Name { get; }
            public string ShortName { get; }
            public IReadOnlyList<IClass> Classes { get; }
        }

        public IReadOnlyList<IAssembly> Assemblies { get; set; }
        public IReadOnlyList<MetricType> MetricTypes { get; set; }

        public void FileRenamed(IReadOnlyList<FileRename> fileRenames)
        {
            foreach (IAssembly assembly in this.Assemblies)
            {
                (assembly as DynamicAssembly).FileRenamed(fileRenames);
            }

            FileRenamedEvent?.Invoke(this, fileRenames);
        }
        public static DynamicReportResult FromReportResult(IReportResult reportResult) => new DynamicReportResult
        {
            Assemblies = reportResult.Assemblies.Select(assembly => new DynamicAssembly(assembly)).ToList(),
            MetricTypes = reportResult.MetricTypes
        };
    }
}
