using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Output
{
    [Export(typeof(IReportTreeExpander))]
    internal class ReportTreeExpander : IReportTreeExpander
    {
        private readonly TreeExpander<ReportTreeItemBase> _treeExpander = new TreeExpander<ReportTreeItemBase>(
            ti => ti.Name, ti => ti.IsExpanded, ti => ti.IsExpanded = true, ti => ti.ReportChildren);

        public void RestoreExpansionState(IList<ReportTreeItemBase> oldItems, IList<ReportTreeItemBase> newItems)
            => this._treeExpander.RestoreExpansionState(oldItems, newItems);
    }
}
