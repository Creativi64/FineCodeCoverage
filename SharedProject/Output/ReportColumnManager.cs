using TreeGrid;

namespace FineCodeCoverage.Output
{
    internal class ReportColumnManager : ColumnManagerBase
    {
        public ColumnData Name { get; } = new ColumnData("Name", 0, true, 450);
        public ColumnData CoverableLines { get; } = new ColumnData("Coverable Lines", 1, true, 120.0, 20);
        public ReportColumnManager() => this.Columns = new ColumnData[] { this.Name, this.CoverableLines };
        internal void ShowRelevantHotspotColumns(string usedParser)
        {

        }
    }
}
