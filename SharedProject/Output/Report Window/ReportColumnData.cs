using System.Windows;
using TreeGrid;

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
    internal class ReportColumnData : ColumnData, IReportColumnData
    {
        public ReportColumnData(
            string reportColumnType,
            string name,
            int displayIndex,
            bool isVisible,
            double width,
            double minWidth = 100,
            HorizontalAlignment initialAlignment = default
        ) : base(name, displayIndex, isVisible, width, minWidth, initialAlignment, initialAlignment)
        {
            ReportColumnType = reportColumnType;
        }
        public ReportColumnData(
            string reportColumnType,
            string name,
            int displayIndex,
            bool isVisible,
            HorizontalAlignment headerAlignment,
            HorizontalAlignment cellAlignment,
            double width,
            double minWidth = 100
        ) : base(name, displayIndex, isVisible, width, minWidth, headerAlignment, cellAlignment)
        {
            ReportColumnType = reportColumnType;
        }
        public string ReportColumnType { get; }
        public bool CanEditCellAlignment { get; set; } = true;

        public const string NameColumnType = "Name";
        public const string CoverableLinesColumnType = "Coverable Lines";
        public const string BlocksCoveredColumnType = "Blocks Covered";
        public const string BlocksNotCoveredColumnType = "Blocks Not Covered";
        public const string NPathComplexityColumnType = "NPath Complexity";
        public const string CyclomaticComplexityColumnType = "Cyclomatic Complexity";
        public const string CrapScoreColumnType = "Crap Score";
        public const string LineCoveragePercentColumnType = "Line coverage percent";

    }
}
