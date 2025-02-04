using TreeGrid;

namespace FineCodeCoverage.Output
{
    internal interface IReportColumnData
    {
        string ReportColumnType { get; }
        string Name { get; set; }
        bool IsVisible { get; set; }
        int DisplayIndex { get; set; }
    }
    internal class ReportColumnData : ColumnData, IReportColumnData
    {
        public ReportColumnData(string reportColumnType, string name, int displayIndex, bool isVisible, double width, double minWidth = 100)
            : base(name, displayIndex, isVisible, width, minWidth)
        {
            ReportColumnType = reportColumnType;
        }

        public string ReportColumnType { get; }

        public const string NameColumnType = "Name";
        public const string CoverableLinesColumnType = "Coverable Lines";
        public const string BlocksCoveredColumnType = "Blocks Covered";
        public const string BlocksNotCoveredColumnType = "Blocks Not Covered";
        public const string NPathComplexityColumnType = "NPath Complexity";
        public const string CyclomaticComplexityColumnType = "Cyclomatic Complexity";
        public const string CrapScoreColumnType = "Crap Score";

    }
}
