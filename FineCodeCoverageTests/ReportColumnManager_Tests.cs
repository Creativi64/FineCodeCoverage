using System.Collections.Generic;
using System.Linq;
using System.Windows;
using AutoMoq;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Core.Utilities.VsThreading;
using FineCodeCoverage.Engine.ReportGenerator;
using FineCodeCoverage.Output;
using FineCodeCoverageTests.TestHelpers;
using Moq;
using NUnit.Framework;
using TreeGrid;

namespace FineCodeCoverageTests
{
    internal class ReportColumnManager_Tests
    {
        private ReportColumnManager CreateReportColumnManager()
        {
            var autoMoqer = new AutoMoqer();
            autoMoqer.SetInstance<IThreadHelper>(new TestThreadHelper());
            return autoMoqer.Create<ReportColumnManager>();
        }

        [Test]
        public void Should_Have_Column_Properties_With_Distinct_DisplayIndex()
        {
            var (columnsFromProperties, _) = GetColumnProperties();
            Assert.That(columnsFromProperties.Count, Is.EqualTo(columnsFromProperties.Select(p => p.DisplayIndex).Distinct().Count()));
        }

        private (List<ColumnData> columns, ReportColumnManager reportColumnManager) GetColumnProperties()
        {
            var reportColumnManager = CreateReportColumnManager();
            var columnsFromProperties = typeof(ReportColumnManager).GetProperties().Where(p => typeof(ColumnData).IsAssignableFrom(p.PropertyType)).Select(p => p.GetValue(reportColumnManager)).OfType<ColumnData>().ToList();
            return (columnsFromProperties, reportColumnManager);
        }

        [Test]
        public void Should_Have_First_Column_The_Name_Column()
        {
            var reportColumnManager = CreateReportColumnManager();
            var firstColumn = reportColumnManager.Columns[0];
            Assert.That(firstColumn.Name, Is.EqualTo("Name"));
            Assert.That(firstColumn.DisplayIndex, Is.EqualTo(0));
        }

        [Test]
        public void Each_Column_Property_Should_Be_In_Columns()
        {
            var (columnProperies, reportColumnManager) = GetColumnProperties();
            Assert.That(reportColumnManager.Columns, Is.EquivalentTo(columnProperies));
        }

        private ReportColumnManager Setup(List<ReportColumnState> reportColumnStates)
        {
            var autoMoqer = new AutoMoqer();
            autoMoqer.SetInstance<IThreadHelper>(new TestThreadHelper());
            var mockColumnStatesStore = new Mock<IColumnStatesStore>();
            mockColumnStatesStore.Setup(columnStatesStore => columnStatesStore.GetColumnStatesAsync()).ReturnsAsync("SerializedColumnStates");
            var mockJsonConvertService = new Mock<IJsonConvertService>();

            mockJsonConvertService.Setup(jsonConvertService => jsonConvertService.DeserializeObject<List<ReportColumnState>>("SerializedColumnStates"))
                .Returns(reportColumnStates);

            autoMoqer.SetInstance(mockColumnStatesStore.Object);
            autoMoqer.SetInstance(mockJsonConvertService.Object);

            return autoMoqer.Create<ReportColumnManager>();
        }

        [Test]
        public void Should_Use_Saved_Json_Serialized_State_For_The_Properties_Of_Initial_Columns()
        {
            
            var reportColumnManager = Setup(new List<ReportColumnState>
                {
                    new ReportColumnState
                    {
                        DisplayIndex = 0,
                        IsVisible = true,
                        Width = 123,
                        ColumnType = ReportColumnData.NameColumnType
                    },
                    new ReportColumnState
                    {
                        DisplayIndex = 1,
                        IsVisible = false,
                        Width = 456,
                        ColumnType = ReportColumnData.CyclomaticComplexityColumnType,
                        CellAlignment = System.Windows.HorizontalAlignment.Stretch,
                        HeaderAlignment = System.Windows.HorizontalAlignment.Stretch
                    },
                    new ReportColumnState
                    {
                        DisplayIndex = 11,
                        IsVisible = false,
                        Width = 456,
                        ColumnType = ReportColumnData.CoverableLinesColumnType
                    },
                    new ReportColumnState
                    {
                        DisplayIndex = 2,
                        IsVisible = true,
                        Width = 333,
                        ColumnType = ReportColumnData.BlocksCoveredColumnType
                    },

                    // unknown column type
                    new ReportColumnState
                    {
                        DisplayIndex = 3,
                        IsVisible = true,
                        ColumnType = "Unknown",
                        Width = 1
                    }
                });
           
            
            Assert.That(reportColumnManager.Name.Width.Value, Is.EqualTo(123));
            Assert.That(reportColumnManager.Name.IsVisible, Is.True);
            Assert.That(reportColumnManager.Name.DisplayIndex, Is.EqualTo(0));

            
            Assert.That(reportColumnManager.CyclomaticComplexity.IsVisible, Is.False);
            Assert.That(reportColumnManager.CyclomaticComplexity.DisplayIndex, Is.EqualTo(1));
            Assert.That(reportColumnManager.CyclomaticComplexity.Width.Value, Is.EqualTo(0)); // not visible
            Assert.That(reportColumnManager.CyclomaticComplexity.HeaderAlignment, Is.EqualTo(HorizontalAlignment.Stretch));
            Assert.That(reportColumnManager.CyclomaticComplexity.CellAlignment, Is.EqualTo(HorizontalAlignment.Stretch));
        }

        [Test]
        public void Should_Set_Validity_Of_MetricColumnData_Columns()
        {
            var reportColumnManager = Setup(new List<ReportColumnState>());
            

            reportColumnManager.ShowRelevantColumns(new List<MetricType> {
                 MetricType.CyclomaticComplexity, MetricType.BlocksNotCovered });

            
            var crapScoreColumn = reportColumnManager.CrapScore;
            var nPathComplexityColumn = reportColumnManager.NPathComplexity;
            var cyclomaticComplexityColumn = reportColumnManager.CyclomaticComplexity;

            Assert.That(crapScoreColumn.IsInvalid, Is.True);
            Assert.That(nPathComplexityColumn.IsInvalid, Is.True);
            Assert.That(cyclomaticComplexityColumn.IsInvalid, Is.False);
            Assert.That(reportColumnManager.BranchCoveragePercentBar.IsInvalid, Is.True);
            Assert.That(reportColumnManager.CoveredBranches.IsInvalid, Is.True);
            Assert.That(reportColumnManager.TotalBranches.IsInvalid, Is.True);
        }

        [Test]
        public void Should_Save_State_On_VsShutdown_From_Column_Properties_And_UserVisible()
        {
            var autoMoqer = new AutoMoqer();
            autoMoqer.SetInstance<IThreadHelper>(new TestThreadHelper());
            var mockColumnStatesStore = new Mock<IColumnStatesStore>();
            
            mockColumnStatesStore.Setup(columnStatesStore => columnStatesStore.GetColumnStatesAsync()).ReturnsAsync("SerializedColumnStates");
            var mockJsonConvertService = new Mock<IJsonConvertService>();
            mockJsonConvertService.Setup(jsonConvertService => jsonConvertService.SerializeObject(It.IsAny<List<ReportColumnState>>())).Returns("SerializedOnSaved");
            var mockVsShutdown = new Mock<IVsShutdown>();


            mockJsonConvertService.Setup(jsonConvertService => jsonConvertService.DeserializeObject<List<ReportColumnState>>("SerializedColumnStates"))
                .Returns(new List<ReportColumnState> { });

            autoMoqer.SetInstance(mockColumnStatesStore.Object);
            autoMoqer.SetInstance(mockJsonConvertService.Object);
            autoMoqer.SetInstance(mockVsShutdown.Object);

            var reportColumnManager = autoMoqer.Create<ReportColumnManager>();
            var nPathComplexityColumn = reportColumnManager.NPathComplexity;
            nPathComplexityColumn.Width = 456;
            nPathComplexityColumn.DisplayIndex = 3;
            nPathComplexityColumn.IsInvalid = true;

            var crapScoreColumn = reportColumnManager.CrapScore;
            crapScoreColumn.IsVisible = false;
            crapScoreColumn.DisplayIndex = 2;
            crapScoreColumn.Width = 111;

            reportColumnManager.SortColumnsArray();

            mockVsShutdown.Raise(vsShutdown => vsShutdown.Shutdown += null, new object[] { null, null });

            mockColumnStatesStore.Verify(columnStatesStore => columnStatesStore.SaveColumnStatesAsync("SerializedOnSaved"), Times.Once);

            var serializeObjectInvocation = mockJsonConvertService.Invocations.Where(invocation => invocation.Method.Name == nameof(JsonConvertService.SerializeObject)).Single();
            var reportColumnStates = (List<ReportColumnState>)serializeObjectInvocation.Arguments[0];
            for(var i = 0; i < reportColumnManager.Columns.Length; i++)
            {
                var column = reportColumnManager.Columns[i] as ReportColumnData;
                var expectedsVisible = column.ReportColumnType != ReportColumnData.CrapScoreColumnType;
                Assert.That(reportColumnStates[i].ColumnType, Is.EqualTo(column.ReportColumnType));
                Assert.That(reportColumnStates[i].IsVisible, Is.EqualTo(expectedsVisible));
                Assert.That(reportColumnStates[i].DisplayIndex, Is.EqualTo(reportColumnManager.Columns[i].DisplayIndex));
                Assert.That(reportColumnStates[i].Width, Is.EqualTo(reportColumnManager.Columns[i].ActualWidth));
            }
            Assert.That(reportColumnStates.Count, Is.EqualTo(reportColumnManager.Columns.Length));
        }
    }
}
