using AutoMoq;
using FineCodeCoverage.Collection.ReportGeneration;
using FineCodeCoverage.Editor.DynamicCoverage;
using FineCodeCoverage.Output;
using Microsoft.VisualStudio.Text;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace FineCodeCoverageTests.Editor.DynamicCoverage.BufferLineCoverageTests
{
    internal class BufferLineCoverage_LastCoverage_Tests
    {
        class TestLastCoverage : ILastCoverage
        {
            public TestLastCoverage(IFileLines fileLines, DateTime textExecutionStartingDate)
            {
                MockFileLineCoverage = new Mock<IFileLineCoverage>();
                MockFileLineCoverage.Setup(fileLineCoverage => fileLineCoverage.GetLines(FilePath.Value)).Returns(fileLines);
                FileLineCoverage = MockFileLineCoverage.Object;
                TestExecutionStartingDate = textExecutionStartingDate;
            }
            public IFileLineCoverage FileLineCoverage { get; }
            public DateTime TestExecutionStartingDate { get; }
            public Mock<IFileLineCoverage> MockFileLineCoverage { get; }
        }

        [Test]
        public void Should_Have_No_Tracked_Lines_If_None_For_The_File_Path()
        {
            var setup = SimpleSetup.Setup();
            var bufferLineCoverage = setup.BufferLineCoverage;
            
            bufferLineCoverage.SetLastCoverage(new TestLastCoverage(null, DateTime.Now));

            Assert.That(bufferLineCoverage.HasCoverage, Is.False);
        }

        [Test]
        public void Should_Use_TrackedLines_If_Not_Out_Of_Date()
        {
            var setup = SimpleSetup.Setup();
            var bufferLineCoverage = setup.BufferLineCoverage;
            var lastWriteTime = DateTime.Now;
            setup.TextInfoMocks.TextInfo.Setup(textInfo => textInfo.GetLastWriteTime()).Returns(lastWriteTime);
            
            var mockTrackedLines = new Mock<ITrackedLines>();
            var dynamicLines = new List<IDynamicLine>();
            mockTrackedLines.Setup(trackedLines => trackedLines.GetLines(0, 5)).Returns(dynamicLines);
            var mockFileLines = new Mock<IFileLines>();
            mockFileLines.SetupGet(fileLines => fileLines.HasTrackedLines).Returns(true);
            mockFileLines.Setup(fileLines => fileLines.GetTrackedLinesIfNotOutOfDate(lastWriteTime)).Returns(mockTrackedLines.Object);
            bufferLineCoverage.SetLastCoverage(new TestLastCoverage(mockFileLines.Object, lastWriteTime));

            Assert.That(bufferLineCoverage.HasCoverage, Is.True);
            Assert.That(dynamicLines, Is.SameAs(bufferLineCoverage.GetLines(0, 5)));
        }

        [Test]
        public void Should_Not_Have_TrackedLines_If_Out_Of_Date()
        {
            var setup = SimpleSetup.Setup();
            var bufferLineCoverage = setup.BufferLineCoverage;
            var lastWriteTime = DateTime.Now;
            setup.TextInfoMocks.TextInfo.Setup(textInfo => textInfo.GetLastWriteTime()).Returns(lastWriteTime);

            var mockFileLines = new Mock<IFileLines>();
            mockFileLines.SetupGet(fileLines => fileLines.HasTrackedLines).Returns(true);
            mockFileLines.Setup(fileLines => fileLines.GetTrackedLinesIfNotOutOfDate(lastWriteTime)).Returns<ITrackedLines>(null);
            bufferLineCoverage.SetLastCoverage(new TestLastCoverage(mockFileLines.Object, lastWriteTime));

            Assert.That(bufferLineCoverage.HasCoverage, Is.False);
        }

        private (BufferLineCoverage bufferLineCoverage, AutoMoqer autoMoqer, Mock<IFileLineCoverage> mockFileLineCoverage) SetLastCovergeHasTrackedLinesFalse(bool outOfDate)
        {
            var setup = SimpleSetup.Setup();
            var mockTrackedLinesFactory = setup.AutoMoqer.GetMock<ITrackedLinesFactory>();
            mockTrackedLinesFactory.Setup(trackedLinesFactory => trackedLinesFactory.Create(It.IsAny<List<ICoberturaLine>>(), It.IsAny<ITextSnapshot>(), FilePath.Value))
                .Returns(new Mock<ITrackedLines>().Object);

            var bufferLineCoverage = setup.BufferLineCoverage;
            var lastWriteTime = DateTime.Now;
            setup.TextInfoMocks.TextInfo.Setup(textInfo => textInfo.GetLastWriteTime()).Returns(lastWriteTime);
            var mockFileLines = new Mock<IFileLines>();
            mockFileLines.SetupGet(fileLines => fileLines.HasTrackedLines).Returns(false);
            var lastCoverage = new TestLastCoverage(mockFileLines.Object, lastWriteTime.AddMinutes(outOfDate ? -1 : 0));
            bufferLineCoverage.SetLastCoverage(lastCoverage);

            return (bufferLineCoverage, setup.AutoMoqer, lastCoverage.MockFileLineCoverage);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Create_TrackedLines_If_HasTrackedLines_False_And_Not_Out_Of_Date(bool outOfDate)
        {
            var (bufferLineCoverage, _,__) = SetLastCovergeHasTrackedLinesFalse(outOfDate);
            
            Assert.That(bufferLineCoverage.HasCoverage, Is.EqualTo(!outOfDate));
        }

        [Test]
        public void Should_Log_When_LastCoverage_Out_Of_Date()
        {
            var (_, autoMoqer, __) = SetLastCovergeHasTrackedLinesFalse(true);

            autoMoqer.Verify<ILogger>(logger => logger.LogFileAndForget($"Not creating editor marks for {FilePath.Value} as coverage is out of date"));
        }

        [Test]
        public void Should_Info_FileLineCoverage_When_LastCoverage_Out_Of_Date()
        {
            var (_, __, mockFileLineCoverage) = SetLastCovergeHasTrackedLinesFalse(true);

            mockFileLineCoverage.Verify(fileLineCoverage => fileLineCoverage.OutOfDate(FilePath.Value));
        }

        [Test]
        public void Should_Not_Throw_When_TextView_Closed_And_No_FileLines()
        {
            var setup = SimpleSetup.Setup();

            setup.TextInfoMocks.TextView.Raise(textView => textView.Closed += null, EventArgs.Empty);
        }

        [Test]
        public void Should_Inform_The_FileLines_When_TextView_Closed()
        {
            var setup = SimpleSetup.Setup();
            var mockTrackedLinesFactory = setup.AutoMoqer.GetMock<ITrackedLinesFactory>();
            mockTrackedLinesFactory.Setup(trackedLinesFactory => trackedLinesFactory.Create(It.IsAny<List<ICoberturaLine>>(), It.IsAny<ITextSnapshot>(), FilePath.Value))
                .Returns(new Mock<ITrackedLines>().Object);

            var bufferLineCoverage = setup.BufferLineCoverage;
            var lastWriteTime = DateTime.Now;
            setup.TextInfoMocks.TextInfo.Setup(textInfo => textInfo.GetLastWriteTime()).Returns(lastWriteTime);
            var mockFileLines = new Mock<IFileLines>();
            mockFileLines.SetupGet(fileLines => fileLines.HasTrackedLines).Returns(false);
            var lastCoverage = new TestLastCoverage(mockFileLines.Object, lastWriteTime);
            bufferLineCoverage.SetLastCoverage(lastCoverage);

            setup.TextInfoMocks.TextView.Raise(textView => textView.Closed += null, EventArgs.Empty);

            mockFileLines.Verify(fileLines => fileLines.TextViewClosed());
        }
    }
}
