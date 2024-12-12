using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace TreeGrid
{
    internal class DragBeforeFirstBlocker
    {
        public DragBeforeFirstBlocker(IColumnManager columnManager,DataGrid GridHeader,Action reorderedCallback)
        {
            GridHeader.ColumnReordered += new EventHandler<DataGridColumnEventArgs>(this.GridHeader_ColumnReordered);
            GridHeader.ColumnHeaderDragStarted += this.GridHeader_ColumnHeaderDragStarted;
            this.columnManager = columnManager;
            this.reorderedCallback = reorderedCallback;
        }

        private List<int> originalDisplayIndices;
        private readonly IColumnManager columnManager;
        private readonly Action reorderedCallback;

        private void GridHeader_ColumnHeaderDragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            originalDisplayIndices = columnManager.Columns.Select(c => c.DisplayIndex).ToList();
        }

        private void GridHeader_ColumnReordered(object sender, DataGridColumnEventArgs e)
        {
            var firstColumn = columnManager.Columns[0];
            if (firstColumn.DisplayIndex != 0)
            {
                for (var i = 0; i < originalDisplayIndices.Count; i++)
                {
                    columnManager.Columns[i].DisplayIndex = originalDisplayIndices[i];
                }
            }
            else
            {
                reorderedCallback();
            }

        }
    }
}
