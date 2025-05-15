using AutoMoq;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Editor.DynamicCoverage;
using FineCodeCoverage.Options;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using System.Linq;

namespace FineCodeCoverageTests.Editor.DynamicCoverage.BufferLineCoverageTests
{
    internal class BufferLineCoverage_EditorCoverageColouringMode_Off_Tests
    {
        private AutoMoqer autoMoqer;
        private BufferLineCoverage bufferLineCoverage;

        [SetUp]
        public void Setup()
        {
           var setup = SetupForCoverageLines.Setup(EditorCoverageColouringMode.Off);
            autoMoqer = setup.AutoMoqer;
            bufferLineCoverage = setup.BufferLineCoverage;
            bufferLineCoverage.Handle(setup.NewCoverageLinesMessage);
        }

        [Test]
        public void Should_Not_HasCoverage_When_Editor_ColouringMode_Off()
        {
            Assert.That(bufferLineCoverage.HasCoverage, Is.False);
        }

        [Test]
        public void Should_Not_Raise_Coverage_Changed_Message()
        {
            autoMoqer.Verify<IEventAggregator>(eventAggregator => eventAggregator.SendMessage(
                    new CoverageChangedMessage(FilePath.Value, null), null), Times.Never());
        }

        [Test]
        public void Should_Have_Empty_Lines()
        {
            Assert.That(bufferLineCoverage.GetLines(0, 5), Is.Empty);
        }
    }

    internal class BufferLineCoverage_EditorCoverageColouringMode_Changed_Tests_
    {
        [Test]
        public void Should_Remove_TrackedLines_When_AppOptions_Changed_And_EditorCoverageColouringMode_Is_Off()
        {
            var setup = SetupForCoverageLines.Setup();
            var autoMoqer = setup.AutoMoqer;
            var bufferLineCoverage = setup.BufferLineCoverage;

            bufferLineCoverage.Handle(setup.NewCoverageLinesMessage);

            Assert.That(bufferLineCoverage.HasCoverage, Is.True);
            AssertHasLines(true);


            var mockEventAggregator = autoMoqer.GetMock<IEventAggregator>();
            mockEventAggregator.Setup(eventAggregator => eventAggregator.SendMessage(new CoverageChangedMessage(FilePath.Value, null), null))
                .Callback(() => Assert.That(bufferLineCoverage.HasCoverage, Is.False));

            autoMoqer.GetMock<IAppOptionsProvider>().Raise(appOptionsProvider => appOptionsProvider.OptionsChanged += null, 
                new EditorCoverageColouringOptions { EditorCoverageColouringMode = EditorCoverageColouringMode.Off});

            mockEventAggregator.VerifyAll();
            AssertHasLines(false);

            void AssertHasLines(bool expectedHasLines)
            {
                Constraint constraint = expectedHasLines ? (Constraint)Is.GreaterThan(0) : Is.EqualTo(0);
                Assert.That(bufferLineCoverage.GetLines(0, 100).Count, constraint);
            }
        }

        [Test]
        public void Should_Not_Remove_TrackedLines_When_AppOptions_Changed_And_EditorCoverageColouringMode_Is_Not_Off()
        {
            var setup = SetupForCoverageLines.Setup();
            var autoMoqer = setup.AutoMoqer;
            var bufferLineCoverage = setup.BufferLineCoverage;
            var mockEventAggregator = autoMoqer.GetMock<IEventAggregator>();

            setup.BufferLineCoverage.Handle(setup.NewCoverageLinesMessage);
            AssertSingleCoverageChangedMessage();

            autoMoqer.GetMock<IAppOptionsProvider>().Raise(appOptionsProvider => appOptionsProvider.OptionsChanged += null, 
                new EditorCoverageColouringOptions { EditorCoverageColouringMode  = EditorCoverageColouringMode.DoNotUseRoslynWhenTextChanges });


            AssertHasLines();
            AssertSingleCoverageChangedMessage();

            void AssertSingleCoverageChangedMessage()
            {
                mockEventAggregator.Verify(eventAggregator => eventAggregator.SendMessage(It.IsAny<CoverageChangedMessage>(), null), Times.Once());
            }

            void AssertHasLines()
            {
                Assert.That(bufferLineCoverage.GetLines(0, 100), Is.Not.Empty);
            }
        }
    }
}
