using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FineCodeCoverage.Engine;
using FineCodeCoverage.Wpf;
using WpfHelpers;

namespace FineCodeCoverage.Output
{
    internal class ReportColumnsManagementViewModel : ObservableBase, IDialogViewModel, ISelectionHandler<EditableColumn>
    {
        private List<EditableColumn> selectedEditableColumns;
        private bool canMoveDown = false;
        private bool canMoveUp = false;
        private readonly IReportColumnManager reportColumnsManager;
        private readonly IMessageBox messageBox;

        public event EventHandler Done;

        public ReportColumnsManagementViewModel(IReportColumnManager reportColumnsManager, IMessageBox messageBox)
        {
            this.Columns = new ObservableCollection<EditableColumn>(reportColumnsManager.GetColumns().Select(rcd => new EditableColumn(rcd)));
            this.OkCommand = new RelayCommand(() =>
            {
                var columnsInError = this.Columns.Where(c => c.Error != null).ToList();
                if (columnsInError.Count > 0)
                {
                    this.ShowError(columnsInError);
                }
                else
                {
                    this.UpdateReportColumnData();
                    Done?.Invoke(this, EventArgs.Empty);
                }

            }, () => true);
            this.CancelCommand = new RelayCommand(() => Done?.Invoke(this, EventArgs.Empty), () => true);


            /*
                Move logic is to RemoveItem and InsertItem

            */


            this.DownCommand = new RelayCommand(() =>
            {
                this.GetSelectedIndices().OrderByDescending(i => i).ToList().ForEach(i => this.Columns.Move(i, i + 1));
                this.SetCanMove();
            }, () => this.canMoveDown);

            this.UpCommand = new RelayCommand(() =>
            {
                this.GetAscendingSelectedIndices().ToList().ForEach(i => this.Columns.Move(i, i - 1));
                this.SetCanMove();
            }, () => this.canMoveUp);
            this.reportColumnsManager = reportColumnsManager;
            this.messageBox = messageBox;
        }
        public System.Windows.Input.ICommand OkCommand { get; }
        public System.Windows.Input.ICommand CancelCommand { get; }
        public RelayCommand UpCommand { get; }
        public RelayCommand DownCommand { get; }

        private void ShowError(List<EditableColumn> columnsInError)
        {
            string caption = "Column Name Error";
            string message = "Please ensure that all columns have a Name";
            if (columnsInError.Count > 1)
            {
                caption += "s";
            }
            else
            {
                message = $"Column {columnsInError[0].Column} has no Display Name";
            }
            this.messageBox.ShowError(message, caption);
        }

        private void UpdateReportColumnData()
        {
            int displayIndex = 0;
            bool displayIndicesChanged = false;
            foreach (EditableColumn column in this.Columns)
            {
                IReportColumnData reportColumnData = column.ReportColumnData;
                reportColumnData.Name = column.Name.Trim();
                reportColumnData.IsVisible = column.IsVisible;
                reportColumnData.CellAlignment = column.CellAlignment;
                reportColumnData.HeaderAlignment = column.HeaderAlignment;
                if (reportColumnData.DisplayIndex != displayIndex)
                {
                    reportColumnData.DisplayIndex = displayIndex;
                    displayIndicesChanged = true;
                }

                displayIndex++;
            }
            if (displayIndicesChanged)
            {
                this.reportColumnsManager.SortColumnsArray();
            }
        }

        private IEnumerable<int> GetSelectedIndices()
        {
            return this.selectedEditableColumns.Select(demoCol => this.Columns.IndexOf(demoCol));
        }
        private IEnumerable<int> GetAscendingSelectedIndices()
        {
            return this.GetSelectedIndices().OrderBy(i => i);
        }

        public ObservableCollection<EditableColumn> Columns { get; }

        private void SetCanMove()
        {
            var ascendingOrderedSelectedIndices = this.GetAscendingSelectedIndices().ToList();
            int first = ascendingOrderedSelectedIndices[0];
            bool newCanMoveUp = first > 1;
            if (newCanMoveUp != this.canMoveUp)
            {
                this.canMoveUp = newCanMoveUp;
                this.UpCommand.NotifyCanExecuteChanged();
            }

            bool newCanMoveDown = first > 0 && ascendingOrderedSelectedIndices.Last() < this.Columns.Count - 1;
            if (newCanMoveDown != this.canMoveDown)
            {
                this.canMoveDown = newCanMoveDown;
                this.DownCommand.NotifyCanExecuteChanged();
            }
        }

        public void SelectionChanged(List<EditableColumn> selectedItems)
        {
            this.selectedEditableColumns = selectedItems;
            if (selectedItems.Count > 0)
            {
                this.SetCanMove();
            }
        }

        // Reset

    }

}
