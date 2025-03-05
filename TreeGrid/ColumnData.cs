using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace TreeGrid
{
    public class ColumnData : ObservableBase
    {
        private static GridLength EmptyGridLength = new GridLength(0.0);
        private static DataGridLength EmptyDataGridLength = new DataGridLength(0.0);
        private int _displayIndex;
        private bool _isVisible;
        internal DataGridLength _actualWidth;
        private GridLength _gridWidth;
        private ListSortDirection? _sortDirection;
        private string _name;
        private bool _isInvalid;

        public ColumnData(
          string name,
          int displayIndex,
          bool isVisible,
          double width,
          double minWidth = 100)
        {
            this.Name = name;
            this.DisplayIndex = displayIndex;
            this.IsVisible = isVisible;
            this.Width = new DataGridLength(width);
            this.MinWidth = minWidth;
        }

        public string Name { get => _name; set =>this.Set<string>(ref this._name, value, nameof(Name)); }

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
        public bool UserIsVisible { get => this._isVisible; }
        public bool IsVisible
        {
            get => !this._isInvalid && this._isVisible;
            set
            {
                this.Set<bool>(ref this._isVisible, value, nameof(IsVisible));
                this.OnPropertyChanged("Width");
                this.OnPropertyChanged("GridWidth");
            }
        }

        public DataGridLength Width
        {
            get => !this.IsVisible ? ColumnData.EmptyDataGridLength : this._actualWidth;
            set
            {
                this.Set<DataGridLength>(ref this._actualWidth, value, nameof(Width));
                this.GridWidth = new GridLength(this._actualWidth.Value);
            }
        }

        public GridLength GridWidth
        {
            get => !this.IsVisible ? ColumnData.EmptyGridLength : this._gridWidth;
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
    }
}
