using FineCodeCoverage.Collection.ReportGeneration;
using FineCodeCoverage.Editor.DynamicCoverage;
using FineCodeCoverage.Utilities.Events;
using FineCodeCoverage.VSAbstractions.OutputWindow;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace FineCodeCoverageTests.Editor.DynamicCoverage.BufferLineCoverageTests
{
    internal class BufferLineCoverage_NewCoverageLinesMessage_Tests
    {
        [Test]
        public void Should_Not_HasCoverage_Initially_When_No_LastCoverage()
        {
            var setup = SimpleSetup.Setup();

            Assert.That(setup.BufferLineCoverage.HasCoverage, Is.False);
        }

        [Test]
        public void Should_GetLines_From_FileLineCoverage()
        {
            var setup = SimpleSetup.Setup();

            var mockFileLineCoverage = new Mock<IFileLineCoverage>();

            setup.BufferLineCoverage.Handle(new NewCoverageLinesMessage(mockFileLineCoverage.Object));

            mockFileLineCoverage.Verify(fileLineCoverage => fileLineCoverage.GetLines(FilePath.Value));
        }

        [Test]
        public void Should_Create_TrackedLines_From_FileLineCoverage_Storing_On_FileLines()
        {
            var setup = SimpleSetup.Setup();
            
            var coberturaLines = new List<ICoberturaLine> { new TestCoberturaLIne(1) };
            var mockFileLines = new Mock<IFileLines>();
            mockFileLines.SetupGet(fileLines => fileLines.Lines).Returns(coberturaLines);
            var newCoverageLinesMessage = SetupNewCoverageLinesMessage.Setup(mockFileLines.Object);
            var trackedLines = new Mock<ITrackedLines>().Object;
            setup.AutoMoqer.Setup<ITrackedLinesFactory, ITrackedLines>(trackedLinesFactory => trackedLinesFactory.Create(coberturaLines, setup.TextInfoMocks.TextSnapshot.Object, FilePath.Value))
                .Returns(trackedLines);
            setup.BufferLineCoverage.Handle(newCoverageLinesMessage);

            mockFileLines.Verify(fileLines => fileLines.SetTrackedLines(trackedLines));
        }

        [Test]
        public void Should_HasCoverage_True_After()
        {
            var setup = SetupForCoverageLines.Setup();
            var bufferLineCoverage = setup.BufferLineCoverage;
            setup.BufferLineCoverage.Handle(setup.NewCoverageLinesMessage);

            Assert.That(setup.BufferLineCoverage.HasCoverage, Is.True);
        }

        [Test]
        public void Should_Send_CoverageChangedMessage_Null_ChangedLineNumbers()
        {
            var setup = SetupForCoverageLines.Setup();

            setup.BufferLineCoverage.Handle(setup.NewCoverageLinesMessage);

            setup.AutoMoqer.Verify<IEventAggregator>(eventAggregator => eventAggregator.SendMessage(
                    new CoverageChangedMessage(FilePath.Value, null), null));
        }

        [Test]
        public void Should_Not_Have_Tracked_Lines_When_No_FileLines()
        {
            var setup = SetupForCoverageLines.Setup();
            var bufferLineCoverage = setup.BufferLineCoverage;

            bufferLineCoverage.Handle(setup.NewCoverageLinesMessage);

            bufferLineCoverage.Handle(SetupNewCoverageLinesMessage.Setup(null));

            Assert.That(bufferLineCoverage.HasCoverage, Is.False);
            setup.AutoMoqer.Verify<IEventAggregator>(eventAggregator => eventAggregator.SendMessage(It.IsAny<CoverageChangedMessage>(), null), Times.Exactly(2));
        }

        [Test]
        public void Should_Log_If_Exception_Creating_TrackedLines()
        {
            var setup = SimpleSetup.Setup();

            var exception = new Exception("exception");
            var mockFileLineCoverage = new Mock<IFileLineCoverage>();
            mockFileLineCoverage.Setup(fileLineCoverage => fileLineCoverage.GetLines(FilePath.Value)).Throws(exception);

            setup.BufferLineCoverage.Handle(new NewCoverageLinesMessage(mockFileLineCoverage.Object));
            setup.AutoMoqer.Verify<ILogger>(logger => logger.LogFileAndForget($"Error creating tracked lines for {FilePath.Value}", exception.ToString()));
        }
    }
}
