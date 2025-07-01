using System;
using System.Collections.Generic;
using System.Linq;
using FineCodeCoverage.Core.Utilities;

namespace FineCodeCoverage.Collection.ReportGeneration
{
    internal sealed class DynamicReportResult : IDynamicReportResult
    {
        public event EventHandler<IReadOnlyList<FileRename>> FileRenamedEvent;

        public sealed class DynamicCoberturaLine : IDynamicCoberturaLine
        {
            public DynamicCoberturaLine(ICoberturaLine coberturaLine, IDynamicCodeElement codeElement)
            {
                Number = coberturaLine.Number;
                OriginalLineNumber = Number;
                CoverageType = coberturaLine.CoverageType;
                CodeElement = codeElement;
            }

            public int OriginalLineNumber { get; }

            public int Number { get; private set; }

            public CoverageType CoverageType { get; }

            public IDynamicCodeElement CodeElement { get; }

            public void LineMoved(int newLineNumber) => Number = newLineNumber;
        }

        public sealed class DynamicCodeElement : IDynamicCodeElement
        {
            public CodeElementType CodeElementType => _codeElement.CodeElementType;

            public string Name => _codeElement.Name;

            public int StartLine => _codeElement.StartLine;

            public string Path { get; set; }

            public IReadOnlyList<ICoberturaLine> Lines { get; }

            public int BlocksCovered => _codeElement.BlocksCovered;

            public int BlocksNotCovered => _codeElement.BlocksNotCovered;

            public int CyclomaticComplexity => _codeElement.CyclomaticComplexity;

            public int NPathComplexity => _codeElement.NPathComplexity;

            public int TotalBranches => _codeElement.TotalBranches;

            public int BranchesCovered => _codeElement.BranchesCovered;

            public decimal CrapScore => _codeElement.CrapScore;

            private readonly ICodeElement _codeElement;

            public DynamicCodeElement(ICodeElement codeElement)
            {
                _codeElement = codeElement;
                Path = codeElement.Path;
                Lines = codeElement.Lines.Select(l => new DynamicCoberturaLine(l, this)).ToList();
            }

            public DynamicCodeElementState State { get; set; }

            public void IsDirty() => State = DynamicCodeElementState.Dirty;

            public void Deleted() => State = DynamicCodeElementState.Deleted;
        }

        public sealed class DynamicClass : IClass
        {
            public DynamicClass(IClass clss)
            {
                DisplayName = clss.DisplayName;
                _fileCodeElements = clss.FileCodeElements.ToDictionary(kvp => kvp.Key, kvp => (IReadOnlyList<DynamicCodeElement>)kvp.Value.Select(ce => new DynamicCodeElement(ce)).ToList());
                SetFileCodeElements();
                CodeElements = _fileCodeElements.Values.SelectMany(ces => ces).ToList();
            }

            private void SetFileCodeElements() => FileCodeElements = _fileCodeElements.ToDictionary(
                   kvp => kvp.Key,
                   kvp => (IReadOnlyList<ICodeElement>)kvp.Value.Cast<ICodeElement>().ToList());

            public string DisplayName { get; }

            private readonly Dictionary<string, IReadOnlyList<DynamicCodeElement>> _fileCodeElements;

            public IReadOnlyDictionary<string, IReadOnlyList<ICodeElement>> FileCodeElements { get; private set; }

            public IReadOnlyList<ICodeElement> CodeElements { get; }

            internal void FileRenamed(IReadOnlyList<FileRename> fileRenames)
            {
                fileRenames = fileRenames.TryUpdateDictionary(_fileCodeElements);
                if (fileRenames.Count > 0)
                {
                    SetFileCodeElements();
                }

                foreach (FileRename fileRename in fileRenames)
                {
                    IReadOnlyList<DynamicCodeElement> codeElements = _fileCodeElements[fileRename.NewFilePath];
                    foreach (DynamicCodeElement dynamicCodeElement in codeElements)
                    {
                        dynamicCodeElement.Path = fileRename.NewFilePath;
                    }
                }
            }
        }

        public sealed class DynamicAssembly : IAssembly
        {
            public DynamicAssembly(IAssembly assembly)
            {
                Name = assembly.Name;
                ShortName = assembly.ShortName;
                Classes = assembly.Classes.Select(cls => new DynamicClass(cls)).ToList();
            }

            public void FileRenamed(IReadOnlyList<FileRename> fileRenames)
            {
                foreach (IClass cls in Classes)
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
            foreach (IAssembly assembly in Assemblies)
            {
                (assembly as DynamicAssembly).FileRenamed(fileRenames);
            }

            FileRenamedEvent?.Invoke(this, fileRenames);
        }

        public static DynamicReportResult FromReportResult(IReportResult reportResult) => new DynamicReportResult
        {
            Assemblies = reportResult.Assemblies.Select(assembly => new DynamicAssembly(assembly)).ToList(),
            MetricTypes = reportResult.MetricTypes,
        };
    }
}
