using System.Collections.Generic;

namespace TreeGrid
{
    public abstract class TreeGridViewModelBase<TTreeItem,TColumnManager> : ObservableBase, ITreeGridViewModel 
        where TTreeItem : ITreeItem
        where TColumnManager: IColumnManager
    {
        public IColumnManager ColumnManager { get => ColumnManagerImpl; }
        protected abstract TColumnManager ColumnManagerImpl { get; set; }
        public IEnumerable<ITreeItem> Items { get; private set; }
        public ITreeItem SelectedTreeViewItem { set => SelectedTreeItem = (TTreeItem)value; }
        protected virtual TTreeItem SelectedTreeItem { get; set; }
        public string TreeViewAutomationName { get; protected set; }

        public void LeafTreeItemDoubleClick(ITreeItem treeItem)
        {
           LeafTreeItemDoubleClick((TTreeItem)treeItem);
        }
        protected virtual void LeafTreeItemDoubleClick(TTreeItem treeItem)
        {

        }
        protected void SetItems(IEnumerable<TTreeItem> items)
        {
            Items = (IEnumerable<ITreeItem>)items;
        }
        public abstract void Sort(int displayIndex);
    }
}
