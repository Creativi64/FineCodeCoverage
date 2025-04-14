using System.Collections.Generic;

namespace FineCodeCoverage.Output
{   
    internal interface ITreeExpander
    {
        void RestoreExpansionState(IList<ReportTreeItemBase> oldItems,IList<ReportTreeItemBase> newItems);
    }
}
