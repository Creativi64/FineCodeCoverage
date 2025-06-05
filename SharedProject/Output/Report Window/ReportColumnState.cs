using System.Windows;

namespace FineCodeCoverage.Output
{
    internal class ReportColumnState
    {
        public string ColumnType { get; set; }

        public bool IsVisible { get; set; }

        public int DisplayIndex { get; set; }

        public double Width { get; set; }

        public HorizontalAlignment HeaderAlignment { get; set; }

        public HorizontalAlignment CellAlignment { get; set; }
    }
}
