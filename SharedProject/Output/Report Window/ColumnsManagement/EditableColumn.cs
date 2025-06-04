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
            Column = reportColumnData.ReportColumnType;
            Name = reportColumnData.Name;
            IsVisible = reportColumnData.IsVisible;
            ReportColumnData = reportColumnData;
            CanEditVisible = reportColumnData.DisplayIndex > 0;
            HeaderAlignment = reportColumnData.HeaderAlignment;
            CellAlignment = reportColumnData.CellAlignment;
            CanEditCellAlignment = reportColumnData.CanEditCellAlignment;
        }

        public bool IsVisible
        {
            get => _isVisible;
            set => Set(ref _isVisible, value);
        }

        public bool CanEditVisible { get; }

        public string Column { get; set; }

        public IReportColumnData ReportColumnData { get; }

        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        public HorizontalAlignment HeaderAlignment
        {
            get => _headerAlignment;
            set => Set(ref _headerAlignment, value);
        }

        public HorizontalAlignment CellAlignment
        {
            get => _cellAlignment;
            set => Set(ref _cellAlignment, value);
        }

        public bool CanEditCellAlignment
        {
            get => _canEditCellAlignment;
            set => Set(ref _canEditCellAlignment, value);
        }

        public string Error => this[nameof(Name)];

        public string this[string columnName]
            => columnName != nameof(Name) ? null :
                Name.Trim().Length == 0 ? "Name is required" : null;
    }
}
