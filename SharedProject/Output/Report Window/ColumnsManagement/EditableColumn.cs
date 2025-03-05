using System.ComponentModel;
using TreeGrid;

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
        }
        private bool _isVisible;
        public bool IsVisible
        {
            get => this._isVisible;
            set
            {
                this.Set(ref this._isVisible, value, nameof(IsVisible));
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
                this.Set(ref this._name, value, nameof(Name));

            }
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
