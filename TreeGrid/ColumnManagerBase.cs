using System.ComponentModel;
using System.Linq;

namespace TreeGrid
{
    public class ColumnManagerBase : ObservableBase, IColumnManager
    {
        public ColumnData[] Columns { get; set; }
        public ColumnManagerBase() { 
        
        }

        public virtual void SortColumnsArray()
        {
            this.Columns = this.Columns.OrderBy(c => c.DisplayIndex).ToArray();
            this.OnPropertyChanged("Columns");
        }

        public virtual void SortColumns(int columnIndex)
        {
            foreach (var column in Columns)
            {
                if (column.DisplayIndex == columnIndex)
                {
                    if (column.SortDirection == null)
                    {
                        column.SortDirection = ListSortDirection.Ascending;
                    }
                    else if (column.SortDirection == ListSortDirection.Ascending)
                    {
                        column.SortDirection = ListSortDirection.Descending;
                    }
                    else
                    {
                        column.SortDirection = ListSortDirection.Ascending;
                    }
                }
                else
                {
                    column.SortDirection = null;
                }
            }
        }

    }
}
