using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Editor.DynamicCoverage;
using FineCodeCoverage.Output;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace FineCodeCoverage.Engine.ReportGenerator
{
    internal enum DynamicCodeElementState {  Original, Dirty, Deleted}
    internal class DynamicReportResult : IReportResult
    {
        public class DynamicCoberturaLine : IDynamicCoberturaLine
        {
            public DynamicCoberturaLine(ICoberturaLine coberturaLine, IDynamicCodeElement codeElement)
            {
                this.Number = coberturaLine.Number;
                this.OriginalLineNumber = this.Number;
                CodeElement = codeElement;
            }
            public int OriginalLineNumber { get; }
            public int Number { get; private set; }
            public CoverageType CoverageType { get; }
            public IDynamicCodeElement CodeElement { get; }

            public void LineMoved(int newLineNumber)
            {
                this.Number = newLineNumber;
            }
        }
        public class DynamicCodeElement : IDynamicCodeElement
        {
            public CodeElementType CodeElementType => codeElement.CodeElementType;
            public string Name => codeElement.Name;
            public int StartLine => codeElement.StartLine;
            public string Path { get; set; }
            public IReadOnlyList<ICoberturaLine> Lines { get; }
            public int BlocksCovered => codeElement.BlocksCovered;
            public int BlocksNotCovered => codeElement.BlocksNotCovered;
            public int CyclomaticComplexity => codeElement.CyclomaticComplexity;
            public int NPathComplexity => codeElement.NPathComplexity;
            public decimal CrapScore => codeElement.CrapScore;
            private readonly ICodeElement codeElement;

            public DynamicCodeElement(ICodeElement codeElement)
            {
                this.codeElement = codeElement;
                this.Path = codeElement.Path;
                this.Lines = codeElement.Lines.Select(l => new DynamicCoberturaLine(l, this)).ToList();
            }

            public DynamicCodeElementState State { get; set; }
            public void IsDirty()
            {
                State = DynamicCodeElementState.Dirty;
            }

            public void Deleted()
            {
                State = DynamicCodeElementState.Deleted;
            }
        }
        public class DynamicClass : IClass
        {
            public DynamicClass(IClass clss)
            {
                this.DisplayName = clss.DisplayName;
                FileCodeElements = clss.FileCodeElements.ToDictionary(kvp => kvp.Key, kvp => ( IReadOnlyList<ICodeElement>) kvp.Value.Select(ce => new DynamicCodeElement(ce)).ToList());
                CodeElements = FileCodeElements.Values.SelectMany(ces => ces).ToList();
            }
            public string DisplayName { get; }
            public IReadOnlyDictionary<string, IReadOnlyList<ICodeElement>> FileCodeElements { get; }
            public IReadOnlyList<ICodeElement> CodeElements { get; }

            internal void FileRenamed(List<FileRename> fileRenames)
            {
                foreach(var codeElement in CodeElements)
                {
                    var dynamicCodeElement = (DynamicCodeElement)codeElement;
                    fileRenames.ForEach(fileRename =>
                    {
                        if(dynamicCodeElement.Path == fileRename.OldFilePath)
                        {
                            dynamicCodeElement.Path = fileRename.NewFilePath;
                        }
                    });
                }
            }
        }
        public class DynamicAssembly : IAssembly
        {
            public DynamicAssembly(IAssembly assembly)
            {
                Name = assembly.Name;
                ShortName = assembly.ShortName;
                Classes = assembly.Classes.Select(cls => new DynamicClass(cls)).ToList();
            }
            public void FileRenamed(List<FileRename> fileRenames)
            {
                foreach(var cls in Classes)
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

        public void FileRenamed(List<FileRename> fileRenames)
        {
            foreach (var assembly in Assemblies)
            {
                (assembly as DynamicAssembly).FileRenamed(fileRenames);
            }
        }
        public static DynamicReportResult FromReportResult(IReportResult reportResult)
        {

            return new DynamicReportResult
            {
                Assemblies = reportResult.Assemblies.Select(assembly => new DynamicAssembly(assembly)).ToList(),
                MetricTypes = reportResult.MetricTypes
            };
        }
    }


    [Export(typeof(IReportGeneratorUtil))]
    internal partial class ReportGeneratorUtil : IReportGeneratorUtil
    {
        private readonly IFCCReportGenerator reportGenerator;
        private readonly ILogger logger;
        private List<string> logs;
        private DynamicReportResult dynamicReportResult;

        [ImportingConstructor]
        public ReportGeneratorUtil(
            IFCCReportGenerator reportGenerator,
            ILogger logger,
            IFileRenameListener fileRenameListener
        )
        {
            this.reportGenerator = reportGenerator;
            this.logger = logger;
            this.reportGenerator.SetLogger(VerbosityLevel.Info, (_, message) => logs.Add(message));
            fileRenameListener.FileRenamedEvent += FileRenameListener_FileRenamedEvent;
        }

        private void FileRenameListener_FileRenamedEvent(List<FileRename> fileRenames)
            => dynamicReportResult?.FileRenamed(fileRenames);

        public async Task<ReportGeneratorResult> GenerateAsync(
            IEnumerable<string> coverOutputFiles,
            string reportOutputFolder,
            CancellationToken cancellationToken)
        {
            logs = new List<string>();
            logs.Add("Report Generator - Output");
            var reportResult = this.reportGenerator.Generate(coverOutputFiles, reportOutputFolder, new List<string> { "Cobertura", "HtmlSummary" });
            await logger.LogAsync(logs);
            this.dynamicReportResult = DynamicReportResult.FromReportResult(reportResult);
            return new ReportGeneratorResult
            {
                ReportResult = this.dynamicReportResult,
                UnifiedXmlFile = Path.Combine(reportOutputFolder, "Cobertura.xml"),
            };
        }
    }
}
