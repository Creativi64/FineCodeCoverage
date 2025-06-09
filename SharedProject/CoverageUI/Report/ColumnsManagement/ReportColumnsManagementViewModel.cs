using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FineCodeCoverage.Engine;
using FineCodeCoverage.Wpf;
using WpfHelpers;

namespace FineCodeCoverage.Output
{
    internal sealed class ReportColumnsManagementViewModel
        : ObservableBase, IDialogViewModel, ISelectionHandler<EditableColumn>
    {
        private readonly IReportColumnManager _reportColumnsManager;
        private readonly IMessageBox _messageBox;
        private List<EditableColumn> _selectedEditableColumns;
        private bool _canMoveDown = false;
        private bool _canMoveUp = false;

        public event EventHandler Done;

        public ReportColumnsManagementViewModel(
            IReportColumnManager reportColumnsManager,
            IMessageBox messageBox)
        {
            Columns = new ObservableCollection<EditableColumn>(reportColumnsManager.GetColumns().Select(rcd => new EditableColumn(rcd)));
            OkCommand = new RelayCommand(
                () =>
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
                },
                () => true);
            CancelCommand = new RelayCommand(() => Done?.Invoke(this, EventArgs.Empty), () => true);

            /*
                Move logic is to RemoveItem and InsertItem

            */

            DownCommand = new RelayCommand(
                () =>
                {
                    GetSelectedIndices().OrderByDescending(i => i).ToList().ForEach(i => Columns.Move(i, i + 1));
                    SetCanMove();
                },
                () => _canMoveDown);

            UpCommand = new RelayCommand(
                () =>
                {
                    GetAscendingSelectedIndices().ToList().ForEach(i => Columns.Move(i, i - 1));
                    SetCanMove();
                },
                () => _canMoveUp);
            _reportColumnsManager = reportColumnsManager;
            _messageBox = messageBox;
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

            _messageBox.ShowError(message, caption);
        }

        private void UpdateReportColumnData()
        {
            int displayIndex = 0;
            bool displayIndicesChanged = false;
            foreach (EditableColumn column in Columns)
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

            if (!displayIndicesChanged)
            {
                return;
            }

            _reportColumnsManager.SortColumnsArray();
        }

        private IEnumerable<int> GetSelectedIndices()
            => _selectedEditableColumns.Select(demoCol => Columns.IndexOf(demoCol));

        private IEnumerable<int> GetAscendingSelectedIndices()
            => GetSelectedIndices().OrderBy(i => i);

        public ObservableCollection<EditableColumn> Columns { get; }

        private void SetCanMove()
        {
            var ascendingOrderedSelectedIndices = GetAscendingSelectedIndices().ToList();
            int first = ascendingOrderedSelectedIndices[0];
            bool newCanMoveUp = first > 1;
            if (newCanMoveUp != _canMoveUp)
            {
                _canMoveUp = newCanMoveUp;
                UpCommand.NotifyCanExecuteChanged();
            }

            bool newCanMoveDown = first > 0 && ascendingOrderedSelectedIndices.Last() < Columns.Count - 1;
            if (newCanMoveDown == _canMoveDown)
            {
                return;
            }

            _canMoveDown = newCanMoveDown;
            DownCommand.NotifyCanExecuteChanged();
        }

        public void SelectionChanged(List<EditableColumn> selectedItems)
        {
            _selectedEditableColumns = selectedItems;
            if (selectedItems.Count == 0)
            {
                return;
            }

            SetCanMove();
        }

        // Reset
    }
}
