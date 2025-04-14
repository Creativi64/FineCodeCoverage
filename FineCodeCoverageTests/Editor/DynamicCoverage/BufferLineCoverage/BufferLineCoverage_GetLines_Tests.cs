using FineCodeCoverage.Editor.DynamicCoverage;
using FineCodeCoverage.Engine.ReportGenerator;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace FineCodeCoverageTests.Editor.DynamicCoverage.BufferLineCoverageTests
{
    internal class BufferLineCoverage_GetLines_Tests
    {
        [Test]
        public void Should_Delegate_GetLines_To_Tracked_Lines()
        {
            var setup = SimpleSetup.Setup();
            var bufferLineCoverage = setup.BufferLineCoverage;

            var mockFileLineCoverage = new Mock<IFileLineCoverage>();
            var coberturaLines = new List<ICoberturaLine> { new TestCoberturaLIne(1) };
            var fileLines = new FileLines(coberturaLines);
            mockFileLineCoverage.Setup(fileLineCoverage => fileLineCoverage.GetLines(FilePath.Value)).Returns(fileLines);
            var mockTrackedLines = new Mock<ITrackedLines>();
            var dynamicLines = new List<IDynamicLine> { new Mock<IDynamicLine>().Object };
            mockTrackedLines.Setup(trackedLines => trackedLines.GetLines(0, 5)).Returns(dynamicLines);
            setup.AutoMoqer.Setup<ITrackedLinesFactory, ITrackedLines>(trackedLinesFactory => trackedLinesFactory.Create(coberturaLines, setup.TextInfoMocks.TextSnapshot.Object, FilePath.Value))
                .Returns(mockTrackedLines.Object);

            bufferLineCoverage.Handle(new NewCoverageLinesMessage(mockFileLineCoverage.Object));

            Assert.That(bufferLineCoverage.GetLines(0, 5), Is.SameAs(dynamicLines));
        }

        [Test]
        public void Should_Return_Empty_Enumerable_If_No_Tracked_Lines()
        {
            var setup = SimpleSetup.Setup();

            Assert.IsEmpty(setup.BufferLineCoverage.GetLines(0, 5).ToList());
        }
    }
}
