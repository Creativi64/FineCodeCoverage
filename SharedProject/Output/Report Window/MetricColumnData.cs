using FineCodeCoverage.Engine.ReportGenerator;
using System.Windows;

namespace FineCodeCoverage.Output
{
    class MetricColumnData : ReportColumnData
    {
        public MetricColumnData(MetricType metricType, string reportColumnType,string name, int displayIndex, bool isVisible, double width, double minWidth = 100)
            : base(reportColumnType, name, displayIndex, isVisible, width, minWidth, HorizontalAlignment.Right)
        {
            this.MetricType = metricType;
        }

        public MetricType MetricType { get; }
    }
}
