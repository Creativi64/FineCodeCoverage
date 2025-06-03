using System.Collections.Generic;
using System.IO;
using System.Linq;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Core.Utilities.Telemetry;
using FineCodeCoverage.Options;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;

namespace FineCodeCoverage.Output
{
    internal class SourceFileTreeItem : ReportTreeItemBase
    {
        private readonly ISourceFile sourceFile;
        public SourceFileTreeItem(ISourceFile sourceFile, SourceFileStructure sourceFileStructure)
        {
            this.sourceFile = sourceFile;
            this.Name = Path.GetFileName(sourceFile.Path);
            sourceFile.HasNewCodeChanged += (_, __) => MainThreadHelper.SwitchAndFileAndForget(
                    FCCFaultEventName.Create<SourceFileTreeItem>("Report"),
                    () => this.HasNewCode = sourceFile.HasNewCode, "sourceFile.HasNewCodeChanged");
            sourceFile.PathChanged += (_, __) => MainThreadHelper.SwitchAndFileAndForget(
                    FCCFaultEventName.Create<SourceFileTreeItem>("Report"),
                    () => this.Name = Path.GetFileName(sourceFile.Path), "sourceFile.PathChanged");

            this.ImageMoniker = KnownMonikers.TextFile;
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
                        namespaceGroup.Select(clss => new ClassTreeItem(clss))
                    ));
                    break;
            }

            foreach (ReportTreeItemBase child in children)
            {
                child.Parent = this;
                this.observableChildren.Add(child);
                this.CoverableLines += child.CoverableLines;
                this.CoveredLines += child.CoveredLines;
                this.NotCoveredLines += child.NotCoveredLines;
                this.PartialLines += child.PartialLines;
                this.NPathComplexity += child.NPathComplexity;
                this.CrapScore += child.CrapScore;
                this.CyclomaticComplexity += child.CyclomaticComplexity;
                this.TotalBranches += child.TotalBranches;
                this.CoveredBranches += child.CoveredBranches;
            }
        }

        private bool hasNewCode;
        public bool HasNewCode
        {
            get => this.hasNewCode;
            set => this.Set(ref this.hasNewCode, value);
        }
        public override ImageMoniker ImageMoniker { get; }
    }
}