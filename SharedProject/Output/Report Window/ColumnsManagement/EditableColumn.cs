using System.ComponentModel;
using System.Windows;
using WpfHelpers;

namespace FineCodeCoverage.Output
{
    internal class EditableColumn : ObservableBase, IDataErrorInfo
    {
        public EditableColumn()
        {
            // throw if not design time
        }
        public EditableColumn(IReportColumnData reportColumnData)
        {
            Column = reportColumnData.ReportColumnType;
            Name = reportColumnData.Name;
            IsVisible = reportColumnData.IsVisible;
            this.ReportColumnData = reportColumnData;
            CanEditVisible = reportColumnData.DisplayIndex > 0;
            this.HeaderAlignment = reportColumnData.HeaderAlignment;
            this.CellAlignment = reportColumnData.CellAlignment;
            this.CanEditCellAlignment = reportColumnData.CanEditCellAlignment;
        }
        private bool _isVisible;
        public bool IsVisible
        {
            get => this._isVisible;
            set
            {
                this.Set(ref this._isVisible, value);
            }
        }

        public bool CanEditVisible { get; }

        public string Column { get; set; }
        private string _name;
        public IReportColumnData ReportColumnData { get; }

        public string Name
        {
            get => this._name;
            set
            {
                this.Set(ref this._name, value);

            }
        }

        private HorizontalAlignment _headerAlignment;
        public HorizontalAlignment HeaderAlignment
        {
            get => this._headerAlignment;
            set => this.Set(ref this._headerAlignment, value);
        }

        private HorizontalAlignment _cellAlignment;
        public HorizontalAlignment CellAlignment
        {
            get => this._cellAlignment;
            set => this.Set(ref this._cellAlignment, value);
        }

        private bool _canEditCellAlignment;
        public bool CanEditCellAlignment
        {
            get => this._canEditCellAlignment;
            set => this.Set(ref this._canEditCellAlignment, value);
        }

        public string Error { get => this[nameof(Name)]; }

        public string this[string columnName]
        {
            get
            {
                if (columnName == nameof(Name))
                {
                    if (Name.Trim().Length == 0)
                    {
                        return "Name is required";
                    }
                }
                return null;
            }
        }
    }

}
