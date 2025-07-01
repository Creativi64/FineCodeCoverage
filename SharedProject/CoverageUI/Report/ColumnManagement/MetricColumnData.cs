using System.Windows;
using FineCodeCoverage.Collection.ReportGeneration;

namespace FineCodeCoverage.Output
{
    internal sealed class MetricColumnData : ReportColumnData
    {
        public MetricColumnData(
           MetricType metricType,
           string reportColumnType,
           string name,
           int displayIndex,
           bool isVisible,
           HorizontalAlignment headerAlignment,
           HorizontalAlignment cellAlignment,
           double width,
           double minWidth = 100)
            : base(reportColumnType, name, displayIndex, isVisible, headerAlignment, cellAlignment, width, minWidth)
            => MetricType = metricType;

        public MetricColumnData(MetricType metricType, string reportColumnType, string name, int displayIndex, bool isVisible, double width, double minWidth = 100, HorizontalAlignment initialAlignment = HorizontalAlignment.Right)
            : base(reportColumnType, name, displayIndex, isVisible, initialAlignment, initialAlignment, width, minWidth)
            => MetricType = metricType;

        public MetricType MetricType { get; }
    }
}
