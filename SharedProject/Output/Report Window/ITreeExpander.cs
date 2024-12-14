using System.Collections.Generic;

namespace FineCodeCoverage.Output
{
    public interface ITreeExpander
    {
        void SaveExpansionState(IList<ReportTreeItemBase> treeItem);
        void RestoreExpansionState(IList<ReportTreeItemBase> treeItem);
    }
}
