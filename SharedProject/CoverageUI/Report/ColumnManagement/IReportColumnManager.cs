using System.Collections.Generic;
using FineCodeCoverage.Collection.ReportGeneration;
using TreeGrid;

namespace FineCodeCoverage.Output
{
    internal interface IReportColumnManager : IColumnManager
    {
        void ShowRelevantColumns(IReadOnlyList<MetricType> metricTypes);

        void SortColumns(int displayIndex);

        IEnumerable<IReportColumnData> GetColumns();
    }
}
