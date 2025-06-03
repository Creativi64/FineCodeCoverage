using System.Windows;

namespace FineCodeCoverage.Output
{
    internal interface IReportColumnData
    {
        string ReportColumnType { get; }
        string Name { get; set; }
        bool IsVisible { get; set; }
        int DisplayIndex { get; set; }
        HorizontalAlignment HeaderAlignment { get; set; }
        HorizontalAlignment CellAlignment { get; set; }
        bool CanEditCellAlignment { get; }
    }
}