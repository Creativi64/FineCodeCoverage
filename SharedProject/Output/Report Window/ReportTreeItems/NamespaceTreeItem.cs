using System.Collections.Generic;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Imaging;

namespace FineCodeCoverage.Output
{
    public class NamespaceTreeItem : ReportTreeItemBase
    {
        public NamespaceTreeItem(string namespaceName, IEnumerable<ReportTreeItemBase> children)
        {
            this.Name = namespaceName;
            foreach (ReportTreeItemBase child in children)
            {
                this.observableChildren.Add(child);
                child.Parent = this;
                this.CoverableLines += child.CoverableLines;
                this.CoveredLines += child.CoveredLines;
                this.NPathComplexity += child.NPathComplexity;
                this.CrapScore += child.CrapScore;
                this.CyclomaticComplexity += child.CyclomaticComplexity;
                this.BlocksCovered += child.BlocksCovered;
                this.BlocksNotCovered += child.BlocksNotCovered;
            }
        }

        public override ImageMoniker ImageMoniker => KnownMonikers.Namespace;
    }
}
