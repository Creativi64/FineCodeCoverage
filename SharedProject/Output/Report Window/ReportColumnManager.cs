using System.Collections.Generic;
using FineCodeCoverage.Engine.ReportGenerator;
using TreeGrid;

namespace FineCodeCoverage.Output
{
    class MetricColumnData : ColumnData
    {
        public MetricColumnData(MetricType metricType, string name, int displayIndex, bool isVisible, double width, double minWidth = 100) : base(name, displayIndex, isVisible, width, minWidth)
        {
            this.MetricType = metricType;
        }

        public MetricType MetricType { get; }
    }
    internal class ReportColumnManager : ColumnManagerBase
    {
        public ColumnData Name { get; } = new ColumnData("Name", 0, true, 450);
        public ColumnData CoverableLines { get; } = new ColumnData("Coverable Lines", 1, true, 100.0, 20);
        public MetricColumnData BlocksCovered { get; } = new MetricColumnData(MetricType.BlocksCovered, "Blocks Covered", 2, true, 100.0, 20);
        public MetricColumnData BlocksNotCovered { get; } = new MetricColumnData(MetricType.BlocksNotCovered, "Blocks Not Covered", 3, true, 120.0, 20);
        public MetricColumnData NPathComplexity { get; } = new MetricColumnData(MetricType.NPath,"NPath Complexity", 4, true, 110.0, 20);
        public MetricColumnData CyclomaticComplexity { get; } = new MetricColumnData(MetricType.CyclomaticComplexity,"Cyclomatic Complexity", 5, true, 130.0, 20);
        public MetricColumnData CrapScore { get; } = new MetricColumnData(MetricType.Crap,"Crap Score", 6, true, 70.0, 20);

        public ReportColumnManager()
        {
            this.Columns =  new ColumnData[] {
                this.Name, 
                this.CoverableLines,
                this.BlocksCovered, 
                this.BlocksNotCovered, 
                this.NPathComplexity, 
                this.CyclomaticComplexity, 
                this.CrapScore };
        }
        internal void ShowRelevantColumns(List<MetricType> metricTypes)
        {
            foreach(var column in this.Columns)
            {
                if (column is MetricColumnData metricColumnData)
                {
                    metricColumnData.IsVisible = metricTypes.Contains(metricColumnData.MetricType);
                }
            }
        }
    }
}
