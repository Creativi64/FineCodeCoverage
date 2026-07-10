using System.Collections.Generic;

namespace TreeGrid
{
    public interface ITreeGridViewModel
    {
        IColumnManager ColumnManager { get; }
        IEnumerable<ITreeItem> Items { get; }
        ITreeItem SelectedTreeViewItem { set; }
        string TreeViewAutomationName { get; }
        void Sort(int displayIndex);
        void LeafTreeItemDoubleClick(ITreeItem treeItem);
        
    }
}
