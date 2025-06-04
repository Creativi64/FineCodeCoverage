using System.Collections.Generic;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;

namespace FineCodeCoverage.Output
{
    public class NamespaceTreeItem : ReportTreeItemBase
    {
        public NamespaceTreeItem(string namespaceName, IEnumerable<ReportTreeItemBase> children)
        {
            this.Name = namespaceName;
            foreach (ReportTreeItemBase child in children)
            {
                this.ObservableChildren.Add(child);
                child.Parent = this;
                this.CoverableLines += child.CoverableLines;
                this.NotCoveredLines += child.NotCoveredLines;
                this.PartialLines += child.PartialLines;
                this.CoveredLines += child.CoveredLines;
                this.NPathComplexity += child.NPathComplexity;
                this.CrapScore += child.CrapScore;
                this.CyclomaticComplexity += child.CyclomaticComplexity;
                this.TotalBranches += child.TotalBranches;
                this.CoveredBranches += child.CoveredBranches;
            }
        }

        public override ImageMoniker ImageMoniker => KnownMonikers.Namespace;
    }
}
