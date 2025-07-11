using FineCodeCoverage.Editor.DynamicCoverage;
using FineCodeCoverage.Utilities.Events;
using FineCodeCoverage.VSAbstractions.OutputWindow;
using Microsoft.VisualStudio.Text;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FineCodeCoverageTests.Editor.DynamicCoverage.BufferLineCoverageTests
{
    internal class BufferLineCoverage_TextChanged_Tests {
        [TestCase(10, new int[] { -1, 9, 10 }, new int[] { 9 })]
        [TestCase(10, new int[] { 9, 10 }, new int[] { })]
        public void Should_Update_TrackedLines_When_Text_Buffer_ChangedOnBackground_And_Send_CoverageChangedMessage_If_Any_Changed_Within_Snapshot(
          int afterLineCount,
          int[] changedLineNumbers,
          int[] expectedMessageChangedLineNumbers
          )
        {
            var setup = SetupForCoverageLines.Setup();

            setup.BufferLineCoverage.Handle((setup.NewCoverageLinesMessage));

            var mockAfterSnapshot = new Mock<ITextSnapshot>();
            mockAfterSnapshot.SetupGet(afterSnapshot => afterSnapshot.LineCount).Returns(afterLineCount);


            var newSpan = new Span(0, 1);
            setup.MockTrackedLines.Setup(
                trackedLines => trackedLines.GetChangedLineNumbers(mockAfterSnapshot.Object, new List<Span> { newSpan })
            ).Returns(changedLineNumbers);

            setup.TextInfoMocks.TextBuffer.Raise(textBuffer => textBuffer.ChangedOnBackground += null, TextContentChangedEventArgsCreator.Create(mockAfterSnapshot.Object, newSpan));

            setup.AutoMoqer.Verify<IEventAggregator>(
                eventAggregator => eventAggregator.SendMessage(
                    It.Is<CoverageChangedMessage>(message =>
                        message.FilePath == FilePath.Value &&
                        message.ChangedLineNumbers != null && message.ChangedLineNumbers.SequenceEqual(expectedMessageChangedLineNumbers))
                    , null
                ), expectedMessageChangedLineNumbers.Length > 0 ? Times.Once() : Times.Never());

        }

        [Test]
        public void Should_Log_When_Exception_Updating_TrackedLines_When_Text_Buffer_ChangedOnBackground()
        {
            var setup = SetupForCoverageLines.Setup();

            setup.BufferLineCoverage.Handle((setup.NewCoverageLinesMessage));

            var exception = new Exception("message");
            setup.MockTrackedLines.Setup(
                trackedLines => trackedLines.GetChangedLineNumbers(It.IsAny<ITextSnapshot>(), It.IsAny<List<Span>>())
            ).Throws(exception);

            setup.TextInfoMocks.TextBuffer.Raise(textBuffer => textBuffer.ChangedOnBackground += null, TextContentChangedEventArgsCreator.Create(new Mock<ITextSnapshot>().Object, new Span(0, 1)));

            setup.AutoMoqer.Verify<ILogger>(logger => logger.LogFileAndForget($"Error updating tracked lines for {FilePath.Value}", exception.ToString()));
        }

        [Test]
        public void Should_Not_Throw_When_Text_Buffer_Changed_And_No_Coverage()
        {
            var setup = SimpleSetup.Setup();

            setup.TextInfoMocks.TextBuffer.Raise(textBuffer => textBuffer.Changed += null, TextContentChangedEventArgsCreator.Create(new Mock<ITextSnapshot>().Object, new Span(0, 0)));
        }
    }
}
