using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Imaging;
using System.Linq;
using System.Collections.Generic;
using FineCodeCoverage.Engine.ReportGenerator;

namespace FineCodeCoverage.Output
{
    public class AssemblyTreeItem : ReportTreeItemBase
    {
        public AssemblyTreeItem(IAssembly assembly, bool isTestAssembly)
        {
            this.Name = assembly.ShortName;
            this.ImageMoniker = isTestAssembly ? KnownMonikers.Test : KnownMonikers.Assembly;
            IEnumerable<NamespaceTreeItem> namespaceTreeItems = assembly.Classes.GroupBy(clss =>
            {
                string[] classNameParts = clss.DisplayName.Split('.');
                string classNamespace = string.Join(".", classNameParts, 0, classNameParts.Length - 1);
                return classNamespace;
            }).Select(namespaceGroup => new NamespaceTreeItem(
                namespaceGroup.Key,
                namespaceGroup.Select(clss => new ClassTreeItem(clss))
                )
                {
                    Parent = this
                }
            );

            foreach (NamespaceTreeItem namespaceTreeItem in namespaceTreeItems)
            {
                this.observableChildren.Add(namespaceTreeItem);
                this.CoverableLines += namespaceTreeItem.CoverableLines;
                this.CoveredLines += namespaceTreeItem.CoveredLines;
                this.NotCoveredLines += namespaceTreeItem.NotCoveredLines;
                this.PartialLines += namespaceTreeItem.PartialLines;

                this.NPathComplexity += namespaceTreeItem.NPathComplexity;
                this.CrapScore += namespaceTreeItem.CrapScore;
                this.CyclomaticComplexity += namespaceTreeItem.CyclomaticComplexity;

                this.BlocksCovered += namespaceTreeItem.BlocksCovered;
                this.BlocksNotCovered += namespaceTreeItem.BlocksNotCovered;
                this.TotalBranches += namespaceTreeItem.TotalBranches;
                this.CoveredBranches += namespaceTreeItem.CoveredBranches;

            }
        }

        public override ImageMoniker ImageMoniker { get; }
    }
}
