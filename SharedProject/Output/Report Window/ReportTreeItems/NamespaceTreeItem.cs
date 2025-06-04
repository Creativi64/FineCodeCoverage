using System.Collections.Generic;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;

namespace FineCodeCoverage.Output
{
    public class NamespaceTreeItem : ReportTreeItemBase
    {
        public NamespaceTreeItem(string namespaceName, IEnumerable<ReportTreeItemBase> children)
        {
            Name = namespaceName;
            foreach (ReportTreeItemBase child in children)
            {
                ObservableChildren.Add(child);
                child.Parent = this;
                CoverableLines += child.CoverableLines;
                NotCoveredLines += child.NotCoveredLines;
                PartialLines += child.PartialLines;
                CoveredLines += child.CoveredLines;
                NPathComplexity += child.NPathComplexity;
                CrapScore += child.CrapScore;
                CyclomaticComplexity += child.CyclomaticComplexity;
                TotalBranches += child.TotalBranches;
                CoveredBranches += child.CoveredBranches;
            }
        }

        public override ImageMoniker ImageMoniker => KnownMonikers.Namespace;
    }
}
