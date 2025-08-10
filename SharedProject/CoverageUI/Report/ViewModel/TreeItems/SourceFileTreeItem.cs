using System.Collections.Generic;
using System.IO;
using System.Linq;
using FineCodeCoverage.Options.Report;
using FineCodeCoverage.Utilities.Telemetry;
using FineCodeCoverage.Utilities.Threading;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;

namespace FineCodeCoverage.Output
{
    internal sealed class SourceFileTreeItem : ReportTreeItemBase
    {
        private bool _hasNewCode;

        public SourceFileTreeItem(ISourceFile sourceFile, SourceFileStructure sourceFileStructure)
        {
            Name = Path.GetFileName(sourceFile.Path);
            sourceFile.HasNewCodeChanged += (_, __) => MainThreadHelper.SwitchAndFileAndForget(
                    FCCFaultEventName.Create<SourceFileTreeItem>("Report"),
                    () => HasNewCode = sourceFile.HasNewCode,
                    "sourceFile.HasNewCodeChanged");
            sourceFile.PathChanged += (_, __) => MainThreadHelper.SwitchAndFileAndForget(
                    FCCFaultEventName.Create<SourceFileTreeItem>("Report"),
                    () => Name = Path.GetFileName(sourceFile.Path),
                    "sourceFile.PathChanged");

            if (sourceFileStructure == SourceFileStructure.AsRequired)
            {
                sourceFileStructure = sourceFile.Classes.Count > 1 ?
                    sourceFile.Classes.Select(c => c.DisplayName).Distinct().Count() > 1 ?
                    SourceFileStructure.NamespaceAndClass :
                    SourceFileStructure.Class : SourceFileStructure.Method;
            }

            IEnumerable<ReportTreeItemBase> children = null;
            switch (sourceFileStructure)
            {
                case SourceFileStructure.Class:
                    children = sourceFile.Classes.Select(clss => new ClassTreeItem(clss));
                    break;
                case SourceFileStructure.Method:
                    children = sourceFile.Classes.SelectMany(c => c.CodeElements.Select(codeElement => new CodeElementTreeItem(codeElement)));
                    break;
                case SourceFileStructure.NamespaceAndClass:
                    children = sourceFile.Classes.GroupBy(clss =>
                    {
                        string[] classNameParts = clss.DisplayName.Split('.');
                        return string.Join(".", classNameParts, 0, classNameParts.Length - 1);
                    }).Select(namespaceGroup => new NamespaceTreeItem(
                        namespaceGroup.Key,
                        namespaceGroup.Select(clss => new ClassTreeItem(clss))));
                    break;
            }

            foreach (ReportTreeItemBase child in children)
            {
                child.Parent = this;
                ObservableChildren.Add(child);
                CoverableLines += child.CoverableLines;
                CoveredLines += child.CoveredLines;
                NotCoveredLines += child.NotCoveredLines;
                PartialLines += child.PartialLines;
                NPathComplexity += child.NPathComplexity;
                CrapScore += child.CrapScore;
                CyclomaticComplexity += child.CyclomaticComplexity;
                TotalBranches += child.TotalBranches;
                CoveredBranches += child.CoveredBranches;
            }
        }

        public bool HasNewCode
        {
            get => _hasNewCode;
            set => Set(ref _hasNewCode, value);
        }

        public override ImageMoniker ImageMoniker => KnownMonikers.TextFile;
    }
}
