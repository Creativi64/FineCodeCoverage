using System.Collections.Generic;

namespace FineCodeCoverage.Output
{
    internal interface IReportTreeExpander
    {
        void RestoreExpansionState(IList<ReportTreeItemBase> oldItems, IList<ReportTreeItemBase> newItems);
    }
}
