using FineCodeCoverage.Engine;
using FineCodeCoverage.Wpf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using TreeGrid;

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
            Columns = new ObservableCollection<EditableColumn>(reportColumnsManager.GetColumns().Select(rcd => new EditableColumn(rcd)));
            OkCommand = new RelayCommand(() =>
            {
                var columnsInError = Columns.Where(c => c.Error != null).ToList();
                if (columnsInError.Count > 0)
                {
                    ShowError(columnsInError);
                }
                else
                {
                    UpdateReportColumnData();
                    Done?.Invoke(this, EventArgs.Empty);
                }

            }, () => true);
            CancelCommand = new RelayCommand(() => Done?.Invoke(this, EventArgs.Empty), () => true);


            /*
                Move logic is to RemoveItem and InsertItem

            */


            DownCommand = new RelayCommand(() =>
            {
                GetSelectedIndices().OrderByDescending(i => i).ToList().ForEach(i => Columns.Move(i, i + 1));
                SetCanMove();
            }, () => canMoveDown);

            UpCommand = new RelayCommand(() =>
            {
                GetAscendingSelectedIndices().ToList().ForEach(i => Columns.Move(i, i - 1));
                SetCanMove();
            }, () => canMoveUp);
            this.reportColumnsManager = reportColumnsManager;
            this.messageBox = messageBox;
        }
        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }
        public RelayCommand UpCommand { get; }
        public RelayCommand DownCommand { get; }

        private void ShowError(List<EditableColumn> columnsInError)
        {
            var caption = "Column Name Error";
            var message = "Please ensure that all columns have a Name";
            if (columnsInError.Count > 1)
            {
                caption += "s";
            }
            else
            {
                message = $"Column {columnsInError[0].Column} has no Display Name";
            }
            messageBox.ShowError(message, caption);
        }

        private void UpdateReportColumnData()
        {
            var displayIndex = 0;
            var displayIndicesChanged = false;
            foreach (var column in Columns)
            {
                var reportColumnData = column.ReportColumnData;
                reportColumnData.Name = column.Name.Trim();
                reportColumnData.IsVisible = column.IsVisible;
                if (reportColumnData.DisplayIndex != displayIndex)
                {
                    reportColumnData.DisplayIndex = displayIndex;
                    displayIndicesChanged = true;
                }

                displayIndex++;
            }
            if (displayIndicesChanged)
            {
                reportColumnsManager.SortColumnsArray();
            }
        }

        private IEnumerable<int> GetSelectedIndices()
        {
            return selectedEditableColumns.Select(demoCol => Columns.IndexOf(demoCol));
        }
        private IEnumerable<int> GetAscendingSelectedIndices()
        {
            return GetSelectedIndices().OrderBy(i => i);
        }

        public ObservableCollection<EditableColumn> Columns { get; }

        internal void SelectionChanged(IList selectedItems)
        {
            selectedEditableColumns = selectedItems.OfType<EditableColumn>().ToList();
            SetCanMove();
        }

        private void SetCanMove()
        {
            var ascendingOrderedSelectedIndices = GetAscendingSelectedIndices().ToList();
            var first = ascendingOrderedSelectedIndices[0];
            var newCanMoveUp = first > 1;
            if (newCanMoveUp != canMoveUp)
            {
                canMoveUp = newCanMoveUp;
                UpCommand.NotifyCanExecuteChanged();
            }

            var newCanMoveDown = first > 0 && ascendingOrderedSelectedIndices.Last() < Columns.Count - 1;
            if (newCanMoveDown != canMoveDown)
            {
                canMoveDown = newCanMoveDown;
                DownCommand.NotifyCanExecuteChanged();
            }
        }

        public void SelectionChanged(List<EditableColumn> selectedItems)
        {
            selectedEditableColumns = selectedItems.OfType<EditableColumn>().ToList();
            SetCanMove();
        }

        // Reset

    }

}
