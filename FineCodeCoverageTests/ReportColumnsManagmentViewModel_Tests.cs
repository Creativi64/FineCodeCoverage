using FineCodeCoverage.Engine;
using FineCodeCoverage.Output;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace FineCodeCoverageTests
{
    internal static class ReportColumnsManagmentViewModelExtensions
    {
        public static void SelectionIndicesChanged(this ReportColumnsManagementViewModel reportColumnsManagementViewModel, IEnumerable<int> selectedColumnIndices)
        {
            var selectedChanged = selectedColumnIndices.Select(i => reportColumnsManagementViewModel.Columns[i]).ToList();
            reportColumnsManagementViewModel.SelectionChanged(selectedChanged);
        }
    }
    internal class ReportColumnsManagmentViewModel_Tests
    {
        private Mock<IReportColumnManager> mockReportColumnsManager;
        private const string FirstColumn = "First";
        private const string SecondColumn = "Second";
        private const string ThirdColumn = "Third";
        private const string FourthColumn = "Fourth";
        private const string FifthColumn = "Fifth";

        [SetUp]
        public void Setup()
        {
            mockReportColumnsManager = new Mock<IReportColumnManager>();
            mockReportColumnsManager.Setup(rcm => rcm.GetColumns()).Returns(new List<IReportColumnData>
            {
                new TestReportColumnData{ ReportColumnType = FirstColumn, DisplayIndex = 0, IsVisible = true, Name = "FirstName"},
                new TestReportColumnData{ ReportColumnType = SecondColumn, DisplayIndex = 1, IsVisible = false, Name = "SecondName"},
                new TestReportColumnData{ ReportColumnType = ThirdColumn, DisplayIndex = 2, IsVisible = false, Name = "ThirdName"},
                new TestReportColumnData{ ReportColumnType = FourthColumn, DisplayIndex = 3, IsVisible = false, Name = "FourthName"},
                new TestReportColumnData{ ReportColumnType = FifthColumn, DisplayIndex = 4, IsVisible = false, Name = "FifthName"},
            });
        }


        class TestReportColumnData : IReportColumnData
        {
            public string ReportColumnType { get; set; }
            public string Name { get; set; }
            public bool IsVisible { get; set; }
            public int DisplayIndex { get; set; }
        }



        [Test]
        public void Should_Have_EditableColumn_For_Each_Column_Of_The_ReportColumnManager_With_Initial_Values()
        {
            var mockReportColumnsManager = new Mock<IReportColumnManager>();
            mockReportColumnsManager.Setup(rcm => rcm.GetColumns()).Returns(new List<IReportColumnData>
            {
                new TestReportColumnData{ ReportColumnType = "First", DisplayIndex = 0, IsVisible = true, Name = "FirstName"},
                new TestReportColumnData{ ReportColumnType = "Second", DisplayIndex = 1, IsVisible = false, Name = "SecondName"},
            });
            var reportColumnsManagementViewModel = new ReportColumnsManagmentViewModel(mockReportColumnsManager.Object,null);

            Assert.That(reportColumnsManagementViewModel.Columns.Count, Is.EqualTo(2));
            Assert.That(reportColumnsManagementViewModel.Columns[0].Name, Is.EqualTo("FirstName"));
            Assert.That(reportColumnsManagementViewModel.Columns[0].Column, Is.EqualTo("First"));
            Assert.That(reportColumnsManagementViewModel.Columns[0].IsVisible, Is.EqualTo(true));

            Assert.That(reportColumnsManagementViewModel.Columns[1].Name, Is.EqualTo("SecondName"));
            Assert.That(reportColumnsManagementViewModel.Columns[1].Column, Is.EqualTo("Second"));
            Assert.That(reportColumnsManagementViewModel.Columns[1].IsVisible, Is.EqualTo(false));

        }

        [Test]
        public void Should_Not_Be_Possible_To_Edit_The_Column_With_DisplayIndex_0()
        {
            var mockReportColumnsManager = new Mock<IReportColumnManager>();
            mockReportColumnsManager.Setup(rcm => rcm.GetColumns()).Returns(new List<IReportColumnData>
            {
                new TestReportColumnData{ ReportColumnType = "First", DisplayIndex = 0, IsVisible = true, Name = "FirstName"},
                new TestReportColumnData{ ReportColumnType = "Second", DisplayIndex = 1, IsVisible = false, Name = "SecondName"},
            });
            var reportColumnsManagementViewModel = new ReportColumnsManagmentViewModel(mockReportColumnsManager.Object, null);

            Assert.That(reportColumnsManagementViewModel.Columns[0].CanEditVisible, Is.False);

            Assert.That(reportColumnsManagementViewModel.Columns[1].CanEditVisible, Is.True);
        }

        [Test]
        public void Should_Not_Be_Able_To_MoveUp_Or_MoveDown_Initially()
        {
            var mockReportColumnsManager = new Mock<IReportColumnManager>();
            var reportColumnsManagementViewModel = new ReportColumnsManagmentViewModel(mockReportColumnsManager.Object, null);

            Assert.That(reportColumnsManagementViewModel.DownCommand.CanExecute(null), Is.False);
            Assert.That(reportColumnsManagementViewModel.UpCommand.CanExecute(null), Is.False);
        }

        class SelectionChangedResult
        {
            public bool UpCanExecuteChanged { get; set; }
            public bool DownCanExecuteChanged { get; set; }
            public bool UpCanExecute { get; set; }
            public bool DownCanExecute { get; set; }
        }

        private SelectionChangedResult SelectionChangedTest(IEnumerable<int> selectedColumnIndices)
        {
            var reportColumnsManagementViewModel = new ReportColumnsManagmentViewModel(mockReportColumnsManager.Object, null);
            var upCanExecuteChanged = false;
            reportColumnsManagementViewModel.UpCommand.CanExecuteChanged += (sender, args) =>
            {
                upCanExecuteChanged = true;
            };
            var downCanExecuteChanged = false;
            reportColumnsManagementViewModel.DownCommand.CanExecuteChanged += (sender, args) =>
            {
                downCanExecuteChanged = true;
            };
            reportColumnsManagementViewModel.SelectionIndicesChanged(selectedColumnIndices);

            return new SelectionChangedResult
            {
                UpCanExecuteChanged = upCanExecuteChanged,
                DownCanExecuteChanged = downCanExecuteChanged,
                UpCanExecute = reportColumnsManagementViewModel.UpCommand.CanExecute(null),
                DownCanExecute = reportColumnsManagementViewModel.DownCommand.CanExecute(null)
            };
        }
        private void SelectionChangedMoveUpTest(IEnumerable<int> columnIndices,bool expectedCanExecute)
        {
            var selectionChangedResult = SelectionChangedTest(columnIndices);

            Assert.That(selectionChangedResult.UpCanExecuteChanged, Is.EqualTo(expectedCanExecute));
            Assert.That(selectionChangedResult.UpCanExecute, Is.EqualTo(expectedCanExecute));
        }

        [Test]
        public void Should_Be_Able_To_Move_Up_Selected_Items_If_Does_Not_Include_Include_First_Or_Second()
        {
            SelectionChangedMoveUpTest(new List<int> { 2, 3 }, true);
            SelectionChangedMoveUpTest(new List<int> { 3, 2 }, true);
        }

        [Test]
        public void Should_Not_Be_Able_To_Move_Up_Selected_Items_If_Includes_First()
        {
            SelectionChangedMoveUpTest(new List<int> { 0, 3 }, false);
            SelectionChangedMoveUpTest(new List<int> { 3, 0 }, false);
        }
        [Test]
        public void Should_Not_Be_Able_To_Move_Up_Selected_Items_If_Includes_Second()
        {
            SelectionChangedMoveUpTest(new List<int> { 1, 3 }, false);
            SelectionChangedMoveUpTest(new List<int> { 3, 1 }, false);
        }

        private void SelectionChangedMoveDownTest(IEnumerable<int> columnIndices, bool expectedCanExecute)
        {
            var selectionChangedResult = SelectionChangedTest(columnIndices);

            Assert.That(selectionChangedResult.DownCanExecuteChanged, Is.EqualTo(expectedCanExecute));
            Assert.That(selectionChangedResult.DownCanExecute, Is.EqualTo(expectedCanExecute));
        }

        [Test]
        public void Should_Be_Able_To_Move_Down_Selected_Items_If_Does_Not_Include_Include_Last_Or_First()
        {
            SelectionChangedMoveDownTest(new List<int> { 1, 2 }, true);
            SelectionChangedMoveDownTest(new List<int> { 2, 1 }, true);
        }

        [Test]
        public void Should_Not_Be_Able_To_Move_Down_Selected_Items_If_Includes_Last()
        {
            SelectionChangedMoveDownTest(new List<int> { 2, 4 },false);
            SelectionChangedMoveDownTest(new List<int> { 4, 2 }, false);
        }

        [Test]
        public void Should_Not_Be_Able_To_Move_Down_Selected_Items_If_Includes_First()
        {
            SelectionChangedMoveDownTest(new List<int> { 0, 1 }, false);
            SelectionChangedMoveDownTest(new List<int> { 1, 0 }, false);
        }

        [Test]
        public void Should_Show_An_Error_If_A_Column_Has_No_Name_And_Not_Commit_When_Commit_Executed()
        {
            var mockMessageBox = new Mock<IMessageBox>();
            var reportColumnsManagementViewModel = new ReportColumnsManagmentViewModel(mockReportColumnsManager.Object, mockMessageBox.Object);
            var originalName = reportColumnsManagementViewModel.Columns[0].ReportColumnData.Name;
            reportColumnsManagementViewModel.Columns[0].Name = "";
            reportColumnsManagementViewModel.OkCommand.Execute(null);
            mockMessageBox.Verify(mb => mb.ShowError("Column First has no Display Name", "Column Name Error"));

            Assert.That(reportColumnsManagementViewModel.Columns[0].ReportColumnData.Name, Is.EqualTo(originalName));
        }

        [Test]
        public void Should_Show_An_Error_If_Multiple_Columns_Has_No_Name_And_Not_Commit_When_Commit_Executed()
        {
            var mockMessageBox = new Mock<IMessageBox>();
            var reportColumnsManagementViewModel = new ReportColumnsManagmentViewModel(mockReportColumnsManager.Object, mockMessageBox.Object);
            var originalName = reportColumnsManagementViewModel.Columns[0].ReportColumnData.Name;
            reportColumnsManagementViewModel.Columns[0].Name = "";
            reportColumnsManagementViewModel.Columns[1].Name = " ";
            reportColumnsManagementViewModel.OkCommand.Execute(null);

            mockMessageBox.Verify(mb => mb.ShowError("Please ensure that all columns have a Name", "Column Name Errors"));

            Assert.That(reportColumnsManagementViewModel.Columns[0].ReportColumnData.Name, Is.EqualTo(originalName));
        }


        [Test]
        public void Should_Set_Edited_Properties_On_ReportColumnData_When_Commit_Executed_And_No_Error()
        {
            var reportColumnsManagementViewModel = new ReportColumnsManagmentViewModel(mockReportColumnsManager.Object, null);
            
            reportColumnsManagementViewModel.Columns[1].IsVisible = true;
            reportColumnsManagementViewModel.Columns[1].Name = " NewName ";
            
            reportColumnsManagementViewModel.OkCommand.Execute(null);

            Assert.That(reportColumnsManagementViewModel.Columns[1].ReportColumnData.Name, Is.EqualTo("NewName"));
            Assert.That(reportColumnsManagementViewModel.Columns[1].ReportColumnData.IsVisible, Is.True);
            
        }

        [Test]
        public void Should_Raise_Done_Event__When_Commit_Executed_And_No_Error()
        {
            var reportColumnsManagementViewModel = new ReportColumnsManagmentViewModel(mockReportColumnsManager.Object, null);
            var doneRaised = false;
            reportColumnsManagementViewModel.Done += (sender, args) => doneRaised = true;
            reportColumnsManagementViewModel.OkCommand.Execute(null);

            Assert.That(doneRaised, Is.True);
        }

        internal class MovingTestCase
        {
            
            public IEnumerable<int> SelectedColumnIndices { get; set; }
            public List<string> ExpectedColumns { get; }

            public MovingTestCase(bool moveUp, IEnumerable<int> selectedColumnIndices,List<string> expectedColumns)
            {
                SelectedColumnIndices = selectedColumnIndices;
                ExpectedColumns = expectedColumns;
                MoveUp = moveUp;
            }

            public bool MoveUp { get; set; }
        }

        

        public static object[] MovingTestCases = new object[]
        {
            //up
            new MovingTestCase(true, new List<int>{ 4}, new List<string>{ FirstColumn, SecondColumn, ThirdColumn, FifthColumn, FourthColumn }),
            new MovingTestCase(true, new List<int>{ 3}, new List<string>{ FirstColumn, SecondColumn, FourthColumn, ThirdColumn, FifthColumn }),
            new MovingTestCase(true, new List<int>{ 2}, new List<string>{ FirstColumn, ThirdColumn, SecondColumn, FourthColumn, FifthColumn }),
            new MovingTestCase(true, new List<int>{ 2,3}, new List<string>{ FirstColumn, ThirdColumn, FourthColumn, SecondColumn, FifthColumn }),
            // selected order reversed
            new MovingTestCase(true, new List<int>{ 3,2}, new List<string>{ FirstColumn, ThirdColumn, FourthColumn, SecondColumn, FifthColumn }),
            new MovingTestCase(true, new List<int>{ 2,4}, new List<string>{ FirstColumn, ThirdColumn, SecondColumn, FifthColumn, FourthColumn }),

            // down
            new MovingTestCase(false, new List<int>{ 1}, new List<string>{ FirstColumn, ThirdColumn, SecondColumn, FourthColumn, FifthColumn }),
        };

        
        [TestCaseSource(nameof(MovingTestCases))]
        public void Moving_Test(MovingTestCase movingTestCase)
        {
            var reportColumnsManagementViewModel = new ReportColumnsManagmentViewModel(mockReportColumnsManager.Object, null);
            reportColumnsManagementViewModel.SelectionIndicesChanged(movingTestCase.SelectedColumnIndices);

            var command = movingTestCase.MoveUp ? reportColumnsManagementViewModel.UpCommand : reportColumnsManagementViewModel.DownCommand;

            command.Execute(null);

            reportColumnsManagementViewModel.OkCommand.Execute(null);

            AssertColumnDisplayIndices(reportColumnsManagementViewModel, movingTestCase.ExpectedColumns);

            mockReportColumnsManager.Verify(rcm => rcm.SortColumnsArray(), Times.Once);
        }

        private static void AssertColumnDisplayIndices(ReportColumnsManagementViewModel reportColumnsManagementViewModel, List<string> expectedColumns)
        {
            Assert.That(expectedColumns.Distinct().Count, Is.EqualTo(reportColumnsManagementViewModel.Columns.Count));
            for (var i = 0; i < expectedColumns.Count; i++)
            {
                var reportColumnData = reportColumnsManagementViewModel.Columns.Select(ec => ec.ReportColumnData).First(rcd => rcd.ReportColumnType == expectedColumns[i]);
                Assert.That(reportColumnData.DisplayIndex, Is.EqualTo(i));
            }
        }

        [Test]
        public void Should_Move_Down_Twice()
        {
            var reportColumnsManagementViewModel = new ReportColumnsManagmentViewModel(mockReportColumnsManager.Object, null);
            reportColumnsManagementViewModel.SelectionIndicesChanged(new List<int> { 1 });

            reportColumnsManagementViewModel.DownCommand.Execute(null);
            reportColumnsManagementViewModel.DownCommand.Execute(null);

            reportColumnsManagementViewModel.OkCommand.Execute(null);

            AssertColumnDisplayIndices(reportColumnsManagementViewModel, new List<string> { FirstColumn, ThirdColumn, FourthColumn, SecondColumn, FifthColumn });
        }

        [Test]
        public void Should_Move_Up_Twice()
        {
            var reportColumnsManagementViewModel = new ReportColumnsManagmentViewModel(mockReportColumnsManager.Object, null);
            reportColumnsManagementViewModel.SelectionIndicesChanged(new List<int> { 4 });

            reportColumnsManagementViewModel.UpCommand.Execute(null);
            reportColumnsManagementViewModel.UpCommand.Execute(null);

            reportColumnsManagementViewModel.OkCommand.Execute(null);

            AssertColumnDisplayIndices(reportColumnsManagementViewModel, new List<string> { FirstColumn, SecondColumn, FifthColumn, ThirdColumn, FourthColumn });
        }

        [Test]
        public void Should_Recalculate_Can_Move_When_Move()
        {
            var reportColumnsManagementViewModel = new ReportColumnsManagmentViewModel(mockReportColumnsManager.Object, null);
            reportColumnsManagementViewModel.SelectionIndicesChanged(new List<int> { 2 });
            var upCanExecuteChanged = false;
            reportColumnsManagementViewModel.UpCommand.CanExecuteChanged += (sender, args) =>
            {
                upCanExecuteChanged = true;
            };
            reportColumnsManagementViewModel.UpCommand.Execute(null);

            Assert.That(upCanExecuteChanged, Is.True);
            Assert.That(reportColumnsManagementViewModel.UpCommand.CanExecute(null), Is.False);

            reportColumnsManagementViewModel.SelectionIndicesChanged(new List<int> { 3 });
            var downCanExecuteChanged = false;
            reportColumnsManagementViewModel.DownCommand.CanExecuteChanged += (sender, args) =>
            {
                downCanExecuteChanged = true;
            };
            reportColumnsManagementViewModel.DownCommand.Execute(null);

            Assert.That(downCanExecuteChanged, Is.True);
            Assert.That(reportColumnsManagementViewModel.DownCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void Should_Not_Change_Display_Indices_If_Do_Not_Move()
        {
            var reportColumnsManagementViewModel = new ReportColumnsManagmentViewModel(mockReportColumnsManager.Object, null);
            reportColumnsManagementViewModel.Columns[0].Name = "new";

            reportColumnsManagementViewModel.OkCommand.Execute(null);


            for(var i = 0; i < reportColumnsManagementViewModel.Columns.Count; i++)
            {
                Assert.That(reportColumnsManagementViewModel.Columns[i].ReportColumnData.DisplayIndex, Is.EqualTo(i));
            }
            mockReportColumnsManager.Verify(rcm => rcm.SortColumnsArray(), Times.Never);
        }

        [Test]
        public void Should_Raise_The_Done_Event_And_Not_Change_ReportColumnData_When_Cancel_Command_Is_Executed()
        {
            var reportColumnsManagementViewModel = new ReportColumnsManagmentViewModel(mockReportColumnsManager.Object, null);
            var doneRaised = false;
            reportColumnsManagementViewModel.Done += (sender, args) => doneRaised = true;

            var originalName = reportColumnsManagementViewModel.Columns[0].Name;
            reportColumnsManagementViewModel.Columns[0].Name = "Changed";
            reportColumnsManagementViewModel.CancelCommand.Execute(null);

            Assert.That(doneRaised, Is.True);
            Assert.That(reportColumnsManagementViewModel.Columns[0].ReportColumnData.Name, Is.EqualTo(originalName));
        }
    }
}
