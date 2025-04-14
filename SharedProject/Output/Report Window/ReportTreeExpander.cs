using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Output
{
    [Export(typeof(IReportTreeExpander))]
    internal class ReportTreeExpander : IReportTreeExpander
    {
        private readonly TreeExpander<ReportTreeItemBase> treeExpander = new TreeExpander<ReportTreeItemBase>(
            ti => ti.Name, ti => ti.IsExpanded, ti => ti.IsExpanded = true, ti => ti.observableChildren);
        public void RestoreExpansionState(IList<ReportTreeItemBase> oldItems, IList<ReportTreeItemBase> newItems)
        {
            treeExpander.RestoreExpansionState(oldItems, newItems);
        }
    }
}
