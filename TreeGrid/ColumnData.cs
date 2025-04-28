using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using WpfHelpers;

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
        private HorizontalAlignment _headerAlignment;
        private HorizontalAlignment _cellAlignment;

        public ColumnData(
          string name,
          int displayIndex,
          bool isVisible,
          double width,
          double minWidth = 100,
          HorizontalAlignment headerAlignment = default,
          HorizontalAlignment cellAlignment = default)
        {
            this.Name = name;
            this.DisplayIndex = displayIndex;
            this.IsVisible = isVisible;
            this.Width = new DataGridLength(width);
            this.MinWidth = minWidth;
            _headerAlignment = headerAlignment;
            _cellAlignment = cellAlignment;
        }

        public string Name { get => _name; set =>this.Set(ref this._name, value); }

        public double MinWidth { get; set; }

        public int DisplayIndex
        {
            get => this._displayIndex;
            set => this.Set(ref this._displayIndex, value);
        }

        public bool IsInvalid
        {
            get => this._isInvalid;
            set
            {
                this._isInvalid = value;
                this.OnPropertyChanged(nameof(IsVisible));
                this.OnPropertyChanged(nameof(Width));
                this.OnPropertyChanged(nameof(GridWidth));
            }
        }
        public bool UserIsVisible { get => this._isVisible; }
        public bool IsVisible
        {
            get => !this._isInvalid && this._isVisible;
            set
            {
                this.Set(ref this._isVisible, value);
                this.OnPropertyChanged(nameof(Width));
                this.OnPropertyChanged(nameof(GridWidth));
            }
        }

        public DataGridLength Width
        {
            get => !this.IsVisible ? ColumnData.EmptyDataGridLength : this._actualWidth;
            set
            {
                this.Set(ref this._actualWidth, value);
                this.GridWidth = new GridLength(this._actualWidth.Value);
            }
        }

        public GridLength GridWidth
        {
            get => !this.IsVisible ? ColumnData.EmptyGridLength : this._gridWidth;
            set => this.Set(ref this._gridWidth, value);
        }

        public ListSortDirection? SortDirection
        {
            get => this._sortDirection;
            set
            {
                this.Set(ref this._sortDirection, value);
                this.OnPropertyChanged(nameof(Visibility));
            }
        }

        public HorizontalAlignment HeaderAlignment
        {
            get => this._headerAlignment;
            set => this.Set(ref this._headerAlignment, value);
        }

        public HorizontalAlignment CellAlignment
        {
            get => this._cellAlignment;
            set => this.Set(ref this._cellAlignment, value);
        }
    }
}
