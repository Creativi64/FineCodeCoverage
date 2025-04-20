using AutoMoq;
using FineCodeCoverage.Editor.DynamicCoverage;
using Microsoft.VisualStudio.Text;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FineCodeCoverageTests.Editor.DynamicCoverage
{
    internal class NewCodeTracker_Tests
    {
        private AutoMoqer autoMoqer;
        private NewCodeTracker newCodeTracker;

        [SetUp]
        public void Setup()
        {
            this.autoMoqer = new AutoMoqer();
            this.newCodeTracker = autoMoqer.Create<NewCodeTracker>();
        }

        [Test]
        public void Should_Have_No_Lines_Initially()
        {
            Assert.That(newCodeTracker.Lines, Is.Empty);
        }

        private Mock<ITrackedNewCodeLine> CreateMockTrackedNewCodeLine(ITextSnapshot textSnapshot, string text, int lineNumber = 10)
        {
            var mockNewDynamicLine = new Mock<IDynamicLine>();
            mockNewDynamicLine.SetupGet(l => l.Number).Returns(lineNumber);
            var newDynamicLine = mockNewDynamicLine.Object;
            var mockTrackedNewCodeLine = new Mock<ITrackedNewCodeLine>();
            mockTrackedNewCodeLine.Setup(trackedNewCodeLine => trackedNewCodeLine.GetText(textSnapshot)).Returns(text);
            mockTrackedNewCodeLine.SetupGet(trackedNewCodeLine => trackedNewCodeLine.Line).Returns(newDynamicLine);
            return mockTrackedNewCodeLine;
        }

        #region SpanAndLineRanges updates
        [Test]
        public void Should_Add_New_TrackedNewCodeLines_For_Distinct_Non_Excluded_New_Start_Lines()
        {
            var textSnapshot = new Mock<ITextSnapshot>().Object;
            var includeTrackedNewCodeLine = CreateMockTrackedNewCodeLine(textSnapshot , "include").Object;
            var excludeTrackedNewCodeLine = CreateMockTrackedNewCodeLine(textSnapshot, "exclude").Object;

            var mockTrackedNewCodeLineFactory = autoMoqer.GetMock<ITrackedNewCodeLineFactory>(MockBehavior.Strict);
            mockTrackedNewCodeLineFactory.Setup(trackedNewCodeLineFactory =>
                trackedNewCodeLineFactory.Create(textSnapshot, SpanTrackingMode.EdgeExclusive, 1)
            ).Returns(includeTrackedNewCodeLine);
            mockTrackedNewCodeLineFactory.Setup(trackedNewCodeLineFactory =>
                trackedNewCodeLineFactory.Create(textSnapshot, SpanTrackingMode.EdgeExclusive, 3)
            ).Returns(excludeTrackedNewCodeLine);

            var mockLineExcluder = autoMoqer.GetMock<ILineExcluder>(MockBehavior.Strict);
            mockLineExcluder.Setup(lineExcluder => lineExcluder.ExcludeIfNotCode("include")).Returns(false);
            mockLineExcluder.Setup(lineExcluder => lineExcluder.ExcludeIfNotCode("exclude")).Returns(true);

            var changedLineNumbers = newCodeTracker.GetChangedLineNumbers(
                textSnapshot,
                new List<LineRange> { 
                    new LineRange(1, 2), 
                    new LineRange(1, 2),
                    new LineRange(3, 4) }
                );
            
            Assert.That(changedLineNumbers.Single(), Is.EqualTo(1));
            Assert.That(newCodeTracker.Lines.Single(), Is.SameAs(includeTrackedNewCodeLine.Line));
        }

        [Test]
        public void Should_Raise_HasNewCodeChanged_True_When_Add_For_The_First_Time()
        {
            var textBuffer = new Mock<ITextBuffer>().Object;
            var mockTextSnapshot = new Mock<ITextSnapshot>();
            mockTextSnapshot.SetupGet(textSnapshot => textSnapshot.TextBuffer).Returns(textBuffer);
            var includeTrackedNewCodeLine = CreateMockTrackedNewCodeLine(mockTextSnapshot.Object, "include").Object;

            var mockTrackedNewCodeLineFactory = autoMoqer.GetMock<ITrackedNewCodeLineFactory>(MockBehavior.Strict);
            mockTrackedNewCodeLineFactory.Setup(trackedNewCodeLineFactory =>
                trackedNewCodeLineFactory.Create(mockTextSnapshot.Object, SpanTrackingMode.EdgeExclusive, 1)
            ).Returns(includeTrackedNewCodeLine);

            var filePath = "filepath";
            var mockTextInfoFactory = autoMoqer.Setup<ITextInfoFactory,string>(
                textInfoFactory => textInfoFactory.GetFilePath(textBuffer)).Returns(filePath);

            HasNewCodeChangedEventArgs hasNewCodeChangedEventArgs = null;
            newCodeTracker.HasNewCodeChanged += (_, args) =>
            {
                hasNewCodeChangedEventArgs = args;
            };

            var changedLineNumbers = newCodeTracker.GetChangedLineNumbers(
                mockTextSnapshot.Object,
                new List<LineRange> { new LineRange(1, 2) }
                );

            Assert.That(hasNewCodeChangedEventArgs.HasNewCode, Is.True);
            Assert.That(hasNewCodeChangedEventArgs.FilePath, Is.EqualTo(filePath));
        }

        private (IEnumerable<int> changeLineNumbers, IEnumerable<IDynamicLine> lines) UpdateTrackedTest(
            TrackedNewCodeLineUpdate trackedNewCodeLineUpdate, LineRange lineRange2 = null, Action beforeUpdate = null)
        {
            var textSnapshot = new Mock<ITextSnapshot>().Object;
            var textSnapshot2 = new Mock<ITextSnapshot>().Object;
            var mockTrackedNewCodeLine = CreateMockTrackedNewCodeLine(textSnapshot, "include");
            mockTrackedNewCodeLine.Setup(trackedNewCodeLine => trackedNewCodeLine.Update(textSnapshot2))
                .Returns(trackedNewCodeLineUpdate);

            autoMoqer.Setup<ITrackedNewCodeLineFactory, ITrackedNewCodeLine>(trackedNewCodeLineFactory =>
                trackedNewCodeLineFactory.Create(textSnapshot, SpanTrackingMode.EdgeExclusive, 1)
            ).Returns(mockTrackedNewCodeLine.Object);

            autoMoqer.Setup<ILineExcluder, bool>(lineExcluder => lineExcluder.ExcludeIfNotCode("include")).Returns(false);

            newCodeTracker.GetChangedLineNumbers(
                textSnapshot,
                new List<LineRange> { new LineRange(1, 1) }
                );

            beforeUpdate?.Invoke();
            var changedLineNumbers = newCodeTracker.GetChangedLineNumbers(
                 textSnapshot2,
                 new List<LineRange> { lineRange2 ?? new LineRange(1, 1) }
                 );
            return (changedLineNumbers, newCodeTracker.Lines);
        }
        

        [Test]
        public void Should_Not_Have_Changed_Lines_When_Line_Exists_And_Not_Updated_Or_Excluded()
        {
            var (changedLineNumbers, lines ) = UpdateTrackedTest(new TrackedNewCodeLineUpdate("include", 1, 1));
            Assert.That(changedLineNumbers, Is.Empty);
            lines.Single();
        }

        [Test]
        public void Should_Have_Changed_Line_When_Line_Exists_And_Excluded()
        {
            var (changedLineNumbers, lines) = UpdateTrackedTest(new TrackedNewCodeLineUpdate("exclude", 1, 1),null, () =>
            {
                autoMoqer.Setup<ILineExcluder, bool>(lineExcluder => lineExcluder.ExcludeIfNotCode("exclude")).Returns(true);
            });
            Assert.That(changedLineNumbers.Single(), Is.EqualTo(1));
            Assert.That(lines, Is.Empty);
        }

        [Test]
        public void Should_Raise_HasNewCodeChanged_False_When_Remove_Last_Tracked()
        {
            HasNewCodeChangedEventArgs hasNewCodeChangedEventArgs = null;
            newCodeTracker.HasNewCodeChanged += (_, args) =>
            {
                hasNewCodeChangedEventArgs = args;
            };
            var (changedLineNumbers, lines) = UpdateTrackedTest(new TrackedNewCodeLineUpdate("exclude", 1, 1), null, () =>
            {
                autoMoqer.Setup<ILineExcluder, bool>(lineExcluder => lineExcluder.ExcludeIfNotCode("exclude")).Returns(true);
            });

            Assert.That(hasNewCodeChangedEventArgs.HasNewCode, Is.False);
        }

        [Test]
        public void Should_Have_Old_And_New_Line_Numbers_When_Line_Number_Updated()
        {
            var (changedLineNumbers, lines) = UpdateTrackedTest(new TrackedNewCodeLineUpdate("include", 2, 1), new LineRange(2,2));
            Assert.That(changedLineNumbers, Is.EqualTo(new int[] { 1,2}));
            lines.Single();
        }



        #endregion

        #region NewCodeCodeRanges updates

        [Test]
        public void Should_Create_From_New_Range_Start_Line_Numbers()
        {
            var textSnapshot = new Mock<ITextSnapshot>().Object;
            List<CodeSpanRange> newCodeSpanRanges = new List<CodeSpanRange> {
                new CodeSpanRange(1,3)
            };
            var trackedNewCodeLine = CreateMockTrackedNewCodeLine(textSnapshot, "", 1).Object;
            autoMoqer.Setup<ITrackedNewCodeLineFactory, ITrackedNewCodeLine>(
                trackedNewCodeLineFactory => trackedNewCodeLineFactory.Create(textSnapshot, SpanTrackingMode.EdgeExclusive, 1)
                ).Returns(trackedNewCodeLine);

            var changedLineNumbers = newCodeTracker.GetChangedLineNumbers(textSnapshot, newCodeSpanRanges);

            Assert.That(changedLineNumbers.Single(), Is.EqualTo(1));
            Assert.That(newCodeTracker.Lines.Single(), Is.SameAs(trackedNewCodeLine.Line));
        }

        [Test]
        public void Should_Have_No_Changes_When_All_CodeSpanRange_Start_Line_Numbers_Are_Already_Tracked()
        {
            var textSnapshot = new Mock<ITextSnapshot>().Object;
            List<CodeSpanRange> newCodeSpanRanges = new List<CodeSpanRange> {
                new CodeSpanRange(1,3)
            };
            var trackedNewCodeLine = CreateMockTrackedNewCodeLine(textSnapshot, "", 1).Object;
            autoMoqer.Setup<ITrackedNewCodeLineFactory, ITrackedNewCodeLine>(
                trackedNewCodeLineFactory => trackedNewCodeLineFactory.Create(textSnapshot, SpanTrackingMode.EdgeExclusive, 1)
                ).Returns(trackedNewCodeLine);

            newCodeTracker.GetChangedLineNumbers(textSnapshot, newCodeSpanRanges);

            var changedLineNumbers = newCodeTracker.GetChangedLineNumbers(textSnapshot, newCodeSpanRanges);
            
            Assert.That(changedLineNumbers.Any(),Is.False);
            Assert.That(newCodeTracker.Lines.Single(), Is.SameAs(trackedNewCodeLine.Line));
        }

        [Test]
        public void Should_Have_Single_Changed_Line_Number_When_CodeSpanRange_Start_Line_Not_Tracked()
        {
            var textSnapshot = new Mock<ITextSnapshot>().Object;
            var currentTextSnapshot = new Mock<ITextSnapshot>().Object;
            List<CodeSpanRange> newCodeSpanRanges = new List<CodeSpanRange> {
                new CodeSpanRange(1,3)
            };
            var trackedNewCodeLine = CreateMockTrackedNewCodeLine(textSnapshot, "", 1).Object;
            autoMoqer.Setup<ITrackedNewCodeLineFactory, ITrackedNewCodeLine>(
                trackedNewCodeLineFactory => trackedNewCodeLineFactory.Create(textSnapshot, SpanTrackingMode.EdgeExclusive, 1)
                ).Returns(trackedNewCodeLine);
            var newTrackedNewCodeLine = CreateMockTrackedNewCodeLine(textSnapshot, "", 5).Object;
            autoMoqer.Setup<ITrackedNewCodeLineFactory, ITrackedNewCodeLine>(
                trackedNewCodeLineFactory => trackedNewCodeLineFactory.Create(currentTextSnapshot, SpanTrackingMode.EdgeExclusive, 5)
                ).Returns(newTrackedNewCodeLine);

            newCodeTracker.GetChangedLineNumbers(textSnapshot, newCodeSpanRanges);

            var changedLineNumbers = newCodeTracker.GetChangedLineNumbers(
                currentTextSnapshot,
                new List<CodeSpanRange> { new CodeSpanRange(1, 3), new CodeSpanRange(5,7) });

            Assert.That(changedLineNumbers.Single(), Is.EqualTo(5));
            Assert.That(newCodeTracker.Lines, Is.EqualTo(new List<IDynamicLine> { trackedNewCodeLine.Line, newTrackedNewCodeLine.Line }));
        }

        [Test]
        public void Should_Remove_Tracked_Lines_That_Are_Not_CodeSpanRange_Start()
        {
            var textSnapshot = new Mock<ITextSnapshot>().Object;
            var currentTextSnapshot = new Mock<ITextSnapshot>().Object;
            List<CodeSpanRange> newCodeSpanRanges = new List<CodeSpanRange> {
                new CodeSpanRange(1,3)
            };
            var trackedNewCodeLine = CreateMockTrackedNewCodeLine(textSnapshot, "", 1).Object;
            autoMoqer.Setup<ITrackedNewCodeLineFactory, ITrackedNewCodeLine>(
                trackedNewCodeLineFactory => trackedNewCodeLineFactory.Create(textSnapshot, SpanTrackingMode.EdgeExclusive, 1)
                ).Returns(trackedNewCodeLine);
            var newTrackedNewCodeLine = CreateMockTrackedNewCodeLine(textSnapshot, "", 5).Object;
            autoMoqer.Setup<ITrackedNewCodeLineFactory, ITrackedNewCodeLine>(
                trackedNewCodeLineFactory => trackedNewCodeLineFactory.Create(currentTextSnapshot, SpanTrackingMode.EdgeExclusive, 5)
                ).Returns(newTrackedNewCodeLine);

            newCodeTracker.GetChangedLineNumbers(textSnapshot, newCodeSpanRanges);

            var changedLineNumbers = newCodeTracker.GetChangedLineNumbers(
                currentTextSnapshot,
                new List<CodeSpanRange> { new CodeSpanRange(5, 7) });

            Assert.That(changedLineNumbers, Is.EqualTo(new int[] { 1, 5}));
            Assert.That(newCodeTracker.Lines, Is.EqualTo(new List<IDynamicLine> { newTrackedNewCodeLine.Line }));

        }
        #endregion

        [Test]
        public void Should_Return_Lines_Ordered_By_Line_Number()
        {
            var textSnapshot = new Mock<ITextSnapshot>().Object;
            var textSnapshot2 = new Mock<ITextSnapshot>().Object;
            var mockFirstNewCodeLine = CreateMockTrackedNewCodeLine(textSnapshot, "include",3);
            mockFirstNewCodeLine.Setup(trackedNewCodeLine => trackedNewCodeLine.Update(textSnapshot2))
                .Returns(new TrackedNewCodeLineUpdate("",3,3));
            var secondNewCodeLine = CreateMockTrackedNewCodeLine(textSnapshot, "include", 1).Object;

            autoMoqer.Setup<ITrackedNewCodeLineFactory, ITrackedNewCodeLine>(trackedNewCodeLineFactory =>
                trackedNewCodeLineFactory.Create(textSnapshot, SpanTrackingMode.EdgeExclusive, 3)
            ).Returns(mockFirstNewCodeLine.Object);
            autoMoqer.Setup<ITrackedNewCodeLineFactory, ITrackedNewCodeLine>(trackedNewCodeLineFactory =>
                trackedNewCodeLineFactory.Create(textSnapshot2, SpanTrackingMode.EdgeExclusive, 1)
            ).Returns(secondNewCodeLine);

            autoMoqer.Setup<ILineExcluder, bool>(lineExcluder => lineExcluder.ExcludeIfNotCode(It.IsAny<string>())).Returns(false);

            newCodeTracker.GetChangedLineNumbers(
                textSnapshot,
                new List<LineRange> { new LineRange(3, 3) }
                );

            var changedLineNumbers = newCodeTracker.GetChangedLineNumbers(
                 textSnapshot2,
                 new List<LineRange> { new LineRange(1, 1), new LineRange(3, 3) }
                 );

            
            Assert.That(newCodeTracker.Lines.Select(dl => dl.Number),Is.EqualTo(new int[] { 1,3}));
        }
    }
}
