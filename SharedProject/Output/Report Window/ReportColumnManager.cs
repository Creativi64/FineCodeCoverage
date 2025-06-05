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
        private readonly IColumnStatesStore _columnStateStore;
        private readonly IJsonConvertService _jsonConvertService;
        private readonly IThreadHelper _threadHelper;

        [ImportingConstructor]
        public ReportColumnManager(
            IVsShutdown vsShutdown,
            IColumnStatesStore columnStateStore,
            IJsonConvertService jsonConvertService,
            IThreadHelper threadHelper)
        {
            _columnStateStore = columnStateStore;
            _jsonConvertService = jsonConvertService;
            _threadHelper = threadHelper;
            string columnStates = threadHelper.JoinableTaskFactory.Run(() => _columnStateStore.GetColumnStatesAsync());
            SetInitialColumns(GetColumnStates(columnStates));

            vsShutdown.Shutdown += VsShutdown_Shutdown;
        }

        private List<ReportColumnState> GetColumnStates(string jsonColumnStates)
            => jsonColumnStates != null
                ? _jsonConvertService.DeserializeObject<List<ReportColumnState>>(jsonColumnStates)
                : new List<ReportColumnState>();

        #region Columns

        // note that the control uses reflection to create bindings - these properties are required
        public ReportColumnData Name { get; } = new ReportColumnData(ReportColumnData.NameColumnType, "Name", 0, true, 450, 100);

        public ReportColumnData CoverableLines { get; } = new ReportColumnData(ReportColumnData.CoverableLinesColumnType, "Coverable Lines", 1, true, 92, 20, HorizontalAlignment.Right);

        public ReportColumnData CoveredLines { get; } = new ReportColumnData(ReportColumnData.CoveredLinesColumnType, "Covered Lines", 2, true, 85, 20, HorizontalAlignment.Right);

        public ReportColumnData NotCoveredLines { get; } = new ReportColumnData(ReportColumnData.NotCoveredLinesColumnType, "Uncovered Lines", 3, true, 96, 20, HorizontalAlignment.Right);

        public ReportColumnData PartialLines { get; } = new ReportColumnData(ReportColumnData.PartialLinesColumnType, "Partial Lines", 4, true, 71, 20, HorizontalAlignment.Right);

        public ReportColumnData LineCoveragePercent { get; } = new ReportColumnData(ReportColumnData.LineCoveragePercentColumnType, "Line Coverage %", 5, true, 100, 20, HorizontalAlignment.Right);

        public ReportColumnData LineCoveragePercentBar { get; } = new ReportColumnData(ReportColumnData.LineCoveragePercentBarColumnType, "Line Coverage %", 6, true, HorizontalAlignment.Center, HorizontalAlignment.Stretch, 100, 20) { CanEditCellAlignment = false };

        public ReportColumnData TotalBranches { get; } = new MetricColumnData(MetricType.Branches, ReportColumnData.TotalBranchesColumnType, "Total Branches", 7, true, 88, 20, HorizontalAlignment.Right);

        public ReportColumnData CoveredBranches { get; } = new MetricColumnData(MetricType.Branches, ReportColumnData.CoveredBranchesColumnType, "Covered Branches", 8, true, 109, 20, HorizontalAlignment.Right);

        public ReportColumnData NotCoveredBranches { get; } = new MetricColumnData(MetricType.Branches, ReportColumnData.NotCoveredBranchesColumnType, "Uncovered Branches", 9, true, 120, 20, HorizontalAlignment.Right);

        public ReportColumnData BranchCoveragePercent { get; } = new MetricColumnData(MetricType.Branches, ReportColumnData.BranchCoveragePercentColumnType, "Branch Coverage %", 10, true, 112, 20, HorizontalAlignment.Right);

        public ReportColumnData BranchCoveragePercentBar { get; } = new MetricColumnData(MetricType.Branches, ReportColumnData.BranchCoveragePercentBarColumnType, "Branch Coverage %", 11, true, HorizontalAlignment.Center, HorizontalAlignment.Stretch, 112, 20) { CanEditCellAlignment = false };

        public ReportColumnData NPathComplexity { get; } = new MetricColumnData(MetricType.NPath, ReportColumnData.NPathComplexityColumnType, "NPath Complexity", 12, true, 115, 20);

        public ReportColumnData CyclomaticComplexity { get; } = new MetricColumnData(MetricType.CyclomaticComplexity, ReportColumnData.CyclomaticComplexityColumnType, "Cyclomatic Complexity", 13, true, 134, 20);

        public ReportColumnData CrapScore { get; } = new MetricColumnData(MetricType.Crap, ReportColumnData.CrapScoreColumnType, "Crap Score", 14, true, 68, 20);

        #endregion

        private void VsShutdown_Shutdown(object sender, System.EventArgs e) => SaveColumnStates();

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
                    Width = c.ActualWidth, // rather than the getter that is dependent upon visibility / validity
                    HeaderAlignment = c.HeaderAlignment,
                    CellAlignment = c.CellAlignment,
                };
            }).ToList();
            string jsonColumnStates = _jsonConvertService.SerializeObject(reportColumnStates);
            _threadHelper.JoinableTaskFactory.Run(() => _columnStateStore.SaveColumnStatesAsync(jsonColumnStates));
        }

        private void SetInitialColumns(List<ReportColumnState> reportColumnStates)
        {
            // could reflect
            var reportColumns = new ReportColumnData[]
            {
                Name, // must be first
                CoverableLines,
                CoveredLines,
                NotCoveredLines,
                PartialLines,
                LineCoveragePercent,
                LineCoveragePercentBar,
                TotalBranches,
                CoveredBranches,
                NotCoveredBranches,
                BranchCoveragePercent,
                BranchCoveragePercentBar,
                NPathComplexity,
                CyclomaticComplexity,
                CrapScore,
            };
            var originalDisplayIndices = reportColumns.Select(c => c.DisplayIndex).ToList();
            Columns = reportColumns;
            Dictionary<string, ReportColumnData> columnsLookup = reportColumns.ToDictionary(c => c.ReportColumnType);

            reportColumnStates.ForEach(columnState =>
            {
                if (!columnsLookup.TryGetValue(columnState.ColumnType, out ReportColumnData column))
                {
                    return;
                }

                column.IsVisible = columnState.IsVisible;
                column.DisplayIndex = columnState.DisplayIndex;
                column.Width = columnState.Width;
                column.HeaderAlignment = columnState.HeaderAlignment;
                column.CellAlignment = columnState.CellAlignment;
            });
            int numDistinctDisplayIndices = reportColumns.Select(c => c.DisplayIndex).Distinct().Count();
            if (numDistinctDisplayIndices == reportColumns.Length)
            {
                return;
            }

            // there are duplicates - reset to original display indices
            for (int i = 0; i < reportColumns.Length; i++)
            {
                reportColumns[i].DisplayIndex = originalDisplayIndices[i];
            }
        }

        public void ShowRelevantColumns(IReadOnlyList<MetricType> metricTypes)
        {
            foreach (ColumnData column in Columns)
            {
                if (column is MetricColumnData metricColumnData)
                {
                    metricColumnData.IsInvalid = !metricTypes.Contains(metricColumnData.MetricType);
                }
            }
        }

        public IEnumerable<IReportColumnData> GetColumns() => Columns.OfType<IReportColumnData>();
    }
}
