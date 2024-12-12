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
            }
        }

        public override ImageMoniker ImageMoniker => KnownMonikers.Namespace;
    }
}
