using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Core.Utilities.VsThreading;
using FineCodeCoverage.Engine.ReportGenerator;
using TreeGrid;

namespace FineCodeCoverage.Output
{
    [Export(typeof(IReportColumnManager))]
    internal class ReportColumnManager : ColumnManagerBase, IReportColumnManager
    {
        private readonly IColumnStatesStore columnStateStore;
        private readonly IJsonConvertService jsonConvertService;
        private readonly IThreadHelper threadHelper;

        [ImportingConstructor]
        public ReportColumnManager(
            IVsShutdown vsShutdown,
            IColumnStatesStore columnStateStore,
            IJsonConvertService jsonConvertService,
            IThreadHelper threadHelper
        )
        {
            this.columnStateStore = columnStateStore;
            this.jsonConvertService = jsonConvertService;
            this.threadHelper = threadHelper;
            var columnStates = threadHelper.JoinableTaskFactory.Run(() => this.columnStateStore.GetColumnStatesAsync());
            SetInitialColumns(GetColumnStates(columnStates));

            vsShutdown.Shutdown += VsShutdown_Shutdown;
        }

        private List<ReportColumnState> GetColumnStates(string jsonColumnStates)
        {
            return jsonColumnStates != null
                ? jsonConvertService.DeserializeObject<List<ReportColumnState>>(jsonColumnStates)
                : new List<ReportColumnState>();
        }

        #region Columns
        // note that the control uses reflection to create bindings - these properties are required
        public ReportColumnData Name { get; } = new ReportColumnData(ReportColumnData.NameColumnType, "Name", 0, true, 450, 100);

        public ReportColumnData CoverableLines { get; } = new ReportColumnData(ReportColumnData.CoverableLinesColumnType, "Coverable Lines", 1, true, 100, 20, HorizontalAlignment.Right);
        public ReportColumnData BlocksCovered { get; } = new MetricColumnData(MetricType.BlocksCovered,ReportColumnData.BlocksCoveredColumnType, "Blocks Covered", 2, true, 100, 20);
        public ReportColumnData BlocksNotCovered { get; } = new MetricColumnData(MetricType.BlocksNotCovered, ReportColumnData.BlocksNotCoveredColumnType, "Blocks Not Covered", 3, true, 125, 20);
        public ReportColumnData NPathComplexity { get; } = new MetricColumnData(MetricType.NPath, ReportColumnData.NPathComplexityColumnType, "NPath Complexity", 4, true, 115, 20);
        public ReportColumnData CyclomaticComplexity { get; } = new MetricColumnData(MetricType.CyclomaticComplexity, ReportColumnData.CyclomaticComplexityColumnType, "Cyclomatic Complexity", 5, true, 140, 20);
        public ReportColumnData CrapScore { get; } = new MetricColumnData(MetricType.Crap, ReportColumnData.CrapScoreColumnType, "Crap Score", 6, true, 75, 20);
        public ReportColumnData LineCoveragePercent { get; } = new ReportColumnData(ReportColumnData.LineCoveragePercentColumnType, "Line Coverage %", 7, true, HorizontalAlignment.Right, HorizontalAlignment.Stretch,100, 20 );
        #endregion

        private void VsShutdown_Shutdown(object sender, System.EventArgs e)
        {
            SaveColumnStates();
        }

        private void SaveColumnStates()
        {
            var reportColumnStates = Columns.Select(c =>
            {
                var reportColumnData = c as ReportColumnData;
                return new ReportColumnState
                {
                    ColumnType = reportColumnData.ReportColumnType,
                    IsVisible = reportColumnData.UserIsVisible,
                    DisplayIndex = c.DisplayIndex,
                    Width = c.Width.Value,
                    HeaderAlignment = c.HeaderAlignment,
                    CellAlignment = c.CellAlignment
                };
            }).ToList();
            var jsonColumnStates = jsonConvertService.SerializeObject(reportColumnStates);
            threadHelper.JoinableTaskFactory.Run(() => columnStateStore.SaveColumnStatesAsync(jsonColumnStates));
        }

        private void SetInitialColumns(List<ReportColumnState> reportColumnStates)
        {
            // could reflect 
            var reportColumns = new ReportColumnData[]{
                Name,// must be first
                CoverableLines,
                BlocksCovered,
                BlocksNotCovered,
                NPathComplexity,
                CyclomaticComplexity,
                CrapScore,
                LineCoveragePercent
            };
            this.Columns = reportColumns;
            var columnsLookup = reportColumns.ToDictionary(c => c.ReportColumnType);

            reportColumnStates.ForEach(columnState =>
            {
                if(columnsLookup.TryGetValue(columnState.ColumnType, out var column))
                {
                    column.IsVisible = columnState.IsVisible;
                    column.DisplayIndex = columnState.DisplayIndex;
                    column.Width = columnState.Width;
                    column.HeaderAlignment = columnState.HeaderAlignment;
                    column.CellAlignment = columnState.CellAlignment;
                }
            });
        }

        public void ShowRelevantColumns(IReadOnlyList<MetricType> metricTypes)
        {
            foreach(var column in this.Columns)
            {
                if (column is MetricColumnData metricColumnData)
                {
                    metricColumnData.IsInvalid = !metricTypes.Contains(metricColumnData.MetricType);
                }
            }
        }

        public IEnumerable<IReportColumnData> GetColumns()
        {
            return this.Columns.OfType<IReportColumnData>();
        }
    }
}
