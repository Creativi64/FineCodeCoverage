using System.ComponentModel;
using System.Windows;
using WpfHelpers;

namespace FineCodeCoverage.Output
{
    internal class EditableColumn : ObservableBase, IDataErrorInfo
    {
        private bool _isVisible;
        private string _name;
        private HorizontalAlignment _headerAlignment;
        private HorizontalAlignment _cellAlignment;
        private bool _canEditCellAlignment;

        public EditableColumn()
        {
            // throw if not design time
        }
        public EditableColumn(IReportColumnData reportColumnData)
        {
            this.Column = reportColumnData.ReportColumnType;
            this.Name = reportColumnData.Name;
            this.IsVisible = reportColumnData.IsVisible;
            this.ReportColumnData = reportColumnData;
            this.CanEditVisible = reportColumnData.DisplayIndex > 0;
            this.HeaderAlignment = reportColumnData.HeaderAlignment;
            this.CellAlignment = reportColumnData.CellAlignment;
            this.CanEditCellAlignment = reportColumnData.CanEditCellAlignment;
        }

        public bool IsVisible
        {
            get => this._isVisible;
            set => this.Set(ref this._isVisible, value);
        }

        public bool CanEditVisible { get; }

        public string Column { get; set; }

        public IReportColumnData ReportColumnData { get; }

        public string Name
        {
            get => this._name;
            set => this.Set(ref this._name, value);
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

        public bool CanEditCellAlignment
        {
            get => this._canEditCellAlignment;
            set => this.Set(ref this._canEditCellAlignment, value);
        }

        public string Error => this[nameof(this.Name)];

        public string this[string columnName]
            => columnName != nameof(this.Name) ? null :
                this.Name.Trim().Length == 0 ? "Name is required" : null;
    }
}