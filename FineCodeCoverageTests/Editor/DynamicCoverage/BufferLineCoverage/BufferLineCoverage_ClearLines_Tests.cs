using FineCodeCoverage.Editor.DynamicCoverage;
using FineCodeCoverage.Utilities.Events;
using Moq;
using NUnit.Framework;

namespace FineCodeCoverageTests.Editor.DynamicCoverage.BufferLineCoverageTests
{
    internal class BufferLineCoverage_ClearLines_Tests
    {
        [Test]
        public void Should_HasCoverage_False_When_ClearLines()
        {
            var setup = SetupForCoverageLines.Setup();
            var bufferLineCoverage = setup.BufferLineCoverage;
            bufferLineCoverage.Handle(setup.NewCoverageLinesMessage);
            Assert.That(bufferLineCoverage.HasCoverage, Is.True);
            VerifyCoverageChangedMessageTimes(1);
            
            bufferLineCoverage.Handle(new ClearLinesMessage());
            VerifyCoverageChangedMessageTimes(2);

            void VerifyCoverageChangedMessageTimes(int times)
            {
                setup.AutoMoqer.Verify<IEventAggregator>(
                    ea => ea.SendMessage(new CoverageChangedMessage(FilePath.Value, null), null),
                    Times.Exactly(times)
                );
            }
        }

        [Test]
        public void Should_Not_Send_CoverageChangedMessage_For_Coverage_Clearing_If_Not_Tracking()
        {
            var setup = SimpleSetup.Setup();

            setup.BufferLineCoverage.Handle(new ClearLinesMessage());

            setup.AutoMoqer.Verify<IEventAggregator>(eventAggregator => eventAggregator.SendMessage(
                It.IsAny<CoverageChangedMessage>(),
                null
                ), Times.Never());

        }
    }
}
