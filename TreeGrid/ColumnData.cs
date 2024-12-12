using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace TreeGrid
{
    public class ColumnData : ObservableBase
    {
        private static GridLength EmpthGridLength = new GridLength(0.0);
        private static DataGridLength EmpthDataGridLength = new DataGridLength(0.0);
        private readonly int _initialDisplayIndex;
        private readonly bool _initialIsVisible;
        private readonly double _initialWidth;
        private int _displayIndex;
        private bool _isVisible;
        private bool _isInvalid;
        internal DataGridLength _actualWidth;
        private GridLength _gridWidth;
        private ListSortDirection? _sortDirection;

        public ColumnData(
          string name,
          int displayIndex,
          bool isVisible,
          double width,
          double minWidth = 100)
        {
            this.Name = name;
            this._initialDisplayIndex = this.DisplayIndex = displayIndex;
            this._initialIsVisible = this.IsVisible = isVisible;
            this.Width = new DataGridLength(width);
            this.MinWidth = minWidth;
            this._initialWidth = width;
        }

        public string Name { get; set; }

        public double MinWidth { get; set; }

        public int DisplayIndex
        {
            get => this._displayIndex;
            set => this.Set<int>(ref this._displayIndex, value, nameof(DisplayIndex));
        }

        public bool IsInvalid
        {
            get => this._isInvalid;
            set
            {
                this._isInvalid = value;
                this.OnPropertyChanged("IsVisible");
                this.OnPropertyChanged("Width");
                this.OnPropertyChanged("GridWidth");
            }
        }

        public bool IsVisible
        {
            get => !this.IsInvalid && this._isVisible;
            set
            {
                this.Set<bool>(ref this._isVisible, value, nameof(IsVisible));
                this.OnPropertyChanged("Width");
                this.OnPropertyChanged("GridWidth");
            }
        }

        public DataGridLength Width
        {
            get => !this.IsVisible ? ColumnData.EmpthDataGridLength : this._actualWidth;
            set
            {
                this.Set<DataGridLength>(ref this._actualWidth, value, nameof(Width));
                this.GridWidth = new GridLength(this._actualWidth.Value);
            }
        }

        public GridLength GridWidth
        {
            get => !this.IsVisible ? ColumnData.EmpthGridLength : this._gridWidth;
            set => this.Set<GridLength>(ref this._gridWidth, value, nameof(GridWidth));
        }

        public ListSortDirection? SortDirection
        {
            get => this._sortDirection;
            set
            {
                this.Set<ListSortDirection?>(ref this._sortDirection, value, nameof(SortDirection));
                this.OnPropertyChanged("Visibility");
            }
        }

        public void Reset()
        {
            this.DisplayIndex = this._initialDisplayIndex;
            this.IsVisible = this._initialIsVisible;
            this.Width = new DataGridLength(this._initialWidth);
        }
    }
}
