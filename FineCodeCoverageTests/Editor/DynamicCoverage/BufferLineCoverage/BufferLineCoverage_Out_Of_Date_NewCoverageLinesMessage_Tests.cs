using FineCodeCoverage.Collection.Messages;
using FineCodeCoverage.Editor.DynamicCoverage;
using FineCodeCoverage.Utilities.Events;
using FineCodeCoverage.VSAbstractions.OutputWindow;
using Microsoft.VisualStudio.Text;
using Moq;
using NUnit.Framework;
using System.Threading;

namespace FineCodeCoverageTests.Editor.DynamicCoverage.BufferLineCoverageTests
{
    internal class BufferLineCoverage_Out_Of_Date_NewCoverageLinesMessage_Tests
    {
        [Test]
        public void Should_Not_Create_TrackedLines_From_NewCoverageLinesMessage_If_Text_Changed_Since_TestExecutionStartingMessage()
        {
            var setup = SetupForCoverageLines.Setup();
            var bufferLineCoverage = setup.BufferLineCoverage;

            bufferLineCoverage.Handle(new TestExecutionStartingMessage());
            Thread.Sleep(1);
            setup.TextInfoMocks.TextBuffer.Raise(textBuffer => textBuffer.ChangedOnBackground += null, new TextContentChangedEventArgs(new Mock<ITextSnapshot>().Object, new Mock<ITextSnapshot>().Object, new EditOptions(), null));

            bufferLineCoverage.Handle(setup.NewCoverageLinesMessage);

            Assert.That(bufferLineCoverage.HasCoverage, Is.False);
            setup.AutoMoqer.Verify<IEventAggregator>(ea => ea.SendMessage(It.IsAny<CoverageChangedMessage>(), null), Times.Never());
        }

        [Test]
        public void Should_Create_TrackedLines_From_NewCoverageLinesMessage_If_Text_Changed_Before_TestExecutionStartingMessage()
        {
            var setup = SetupForCoverageLines.Setup();
            var bufferLineCoverage = setup.BufferLineCoverage;

            setup.TextInfoMocks.TextBuffer.Raise(textBuffer => textBuffer.ChangedOnBackground += null, new TextContentChangedEventArgs(new Mock<ITextSnapshot>().Object, new Mock<ITextSnapshot>().Object, new EditOptions(), null));
            Thread.Sleep(1);
            bufferLineCoverage.Handle(new TestExecutionStartingMessage());

            bufferLineCoverage.Handle(setup.NewCoverageLinesMessage);

            Assert.That(bufferLineCoverage.HasCoverage, Is.True);
        }


        [Test]
        public void Should_Log_When_Text_Changed_Since_TestExecutionStartingMessage()
        {
            var setup = SetupForCoverageLines.Setup();
            var bufferLineCoverage = setup.BufferLineCoverage;
            
            bufferLineCoverage.Handle(new TestExecutionStartingMessage());
            Thread.Sleep(1);
            setup.TextInfoMocks.TextBuffer.Raise(textBuffer => textBuffer.ChangedOnBackground += null, new TextContentChangedEventArgs(new Mock<ITextSnapshot>().Object, new Mock<ITextSnapshot>().Object, new EditOptions(), null));

            bufferLineCoverage.Handle(setup.NewCoverageLinesMessage);

            setup.AutoMoqer.Verify<ILogger>(ILogger => ILogger.LogFileAndForget($"Not creating editor marks for {FilePath.Value} as it was changed after test execution started"));
        }

        [Test]
        public void Should_Remove_Existing_TrackedLines_When_NewCoverageLinesMessage_If_Text_Changed_Since_TestExecutionStartingMessage()
        {
            var setup = SetupForCoverageLines.Setup();
            var bufferLineCoverage = setup.BufferLineCoverage;

            bufferLineCoverage.Handle(setup.NewCoverageLinesMessage);
            Assert.That(bufferLineCoverage.HasCoverage, Is.True);


            bufferLineCoverage.Handle(new TestExecutionStartingMessage());
            Thread.Sleep(1);
            setup.TextInfoMocks.TextBuffer.Raise(textBuffer => textBuffer.ChangedOnBackground += null, TextContentChangedEventArgsCreator.Create(new Mock<ITextSnapshot>().Object));
            bufferLineCoverage.Handle(setup.NewCoverageLinesMessage);

            Assert.That(bufferLineCoverage.HasCoverage, Is.False);
        }
    }
}
