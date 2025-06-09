using System.Collections.Generic;
using System.Linq;
using FineCodeCoverage.Engine.ReportGenerator;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;

namespace FineCodeCoverage.Output
{
    public class AssemblyTreeItem : ReportTreeItemBase
    {
        public AssemblyTreeItem(IAssembly assembly, bool isTestAssembly)
        {
            Name = assembly.ShortName;
            ImageMoniker = isTestAssembly ? KnownMonikers.Test : KnownMonikers.Assembly;
            IEnumerable<NamespaceTreeItem> namespaceTreeItems = assembly.Classes.GroupBy(clss =>
            {
                string[] classNameParts = clss.DisplayName.Split('.');
                string classNamespace = string.Join(".", classNameParts, 0, classNameParts.Length - 1);
                return classNamespace;
            }).Select(namespaceGroup => new NamespaceTreeItem(
                namespaceGroup.Key,
                namespaceGroup.Select(clss => new ClassTreeItem(clss)))
            {
                Parent = this,
            });

            foreach (NamespaceTreeItem namespaceTreeItem in namespaceTreeItems)
            {
                ObservableChildren.Add(namespaceTreeItem);
                CoverableLines += namespaceTreeItem.CoverableLines;
                CoveredLines += namespaceTreeItem.CoveredLines;
                NotCoveredLines += namespaceTreeItem.NotCoveredLines;
                PartialLines += namespaceTreeItem.PartialLines;

                NPathComplexity += namespaceTreeItem.NPathComplexity;
                CrapScore += namespaceTreeItem.CrapScore;
                CyclomaticComplexity += namespaceTreeItem.CyclomaticComplexity;

                TotalBranches += namespaceTreeItem.TotalBranches;
                CoveredBranches += namespaceTreeItem.CoveredBranches;
            }
        }

        public override ImageMoniker ImageMoniker { get; }
    }
}
