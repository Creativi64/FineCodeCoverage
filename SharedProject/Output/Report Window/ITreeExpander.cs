using System.Collections.Generic;

namespace FineCodeCoverage.Output
{
    public interface ITreeExpander
    {
        void RestoreExpansionState(IList<ReportTreeItemBase> oldItems,IList<ReportTreeItemBase> newItems);
    }
}
