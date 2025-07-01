using FineCodeCoverage.Collection.ReportGeneration;
using FineCodeCoverage.Editor.DynamicCoverage;
using Microsoft.VisualStudio.Text;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace FineCodeCoverageTests.Editor.DynamicCoverage
{
    internal class TrackedCoverageLines_Tests
    {
        [Test]
        public void Should_Update_All_CoverageLine()
        {
            var textSnapshot = new Mock<ITextSnapshot>().Object;
            Mock<ITrackedCoverageLine> CreateMockCoverageLine(List<int> updatedCoverageLines)
            {
                var mockCoverageLine = new Mock<ITrackedCoverageLine>();
                mockCoverageLine.Setup(coverageLine => coverageLine.GetUpdateLineNumbers(textSnapshot)).Returns(updatedCoverageLines);
                return mockCoverageLine;
            }

            var mockCoverageLines = new List<Mock<ITrackedCoverageLine>>
            {
                CreateMockCoverageLine(new List<int>{ 1,2}),
                CreateMockCoverageLine(new List<int>{3,4})
            };

            var trackedCoverageLines = new TrackedCoverageLines(mockCoverageLines.Select(mock => mock.Object).ToList());


            var updatedLineNumbers = trackedCoverageLines.GetUpdatedLineNumbers(textSnapshot).ToList();

            mockCoverageLines.ForEach(mock => mock.Verify());

            Assert.That(updatedLineNumbers, Is.EqualTo(new List<int> { 1,2,3,4}));
        }

        [Test]
        public void Should_Return_Lines_From_CoverageLines()
        {
            var textSnapshot = new Mock<ITextSnapshot>().Object;
            (ITrackedCoverageLine, IDynamicLine) SetUpCoverageLine()
            {
                var mockCoverageLine = new Mock<ITrackedCoverageLine>();
                var dynamicLine = new Mock<IDynamicLine>().Object;
                mockCoverageLine.SetupGet(coverageLine => coverageLine.Line).Returns(dynamicLine);
                return (mockCoverageLine.Object, dynamicLine);
            }

            var (firstCoverageLine, firstDynamicLine) = SetUpCoverageLine();
            var (secondCoverageLine, secondDynamicLine) = SetUpCoverageLine();
            var trackedCoverageLines = new TrackedCoverageLines(new List<ITrackedCoverageLine> { firstCoverageLine, secondCoverageLine});

            var lines = trackedCoverageLines.Lines.ToList();

            Assert.That(lines.Count(), Is.EqualTo(2));
            Assert.That(lines[0], Is.SameAs(firstDynamicLine));
            Assert.That(lines[1], Is.SameAs(secondDynamicLine));
        }

        [Test]
        public void Should_GetFirstTrackedCoverageLineInfo_From_First_TrackedCoverageLine()
        {
            var firstDynamicCoberturaLine = new Mock<IDynamicCoberturaLine>().Object;
            var mockFirstTrackedCoverageLine = new Mock<ITrackedCoverageLine>();
            mockFirstTrackedCoverageLine.SetupGet(trackedCoverageLine => trackedCoverageLine.DynamicCoberturaLine).Returns(firstDynamicCoberturaLine);
            var mockDynamicLine = new Mock<IDynamicLine>();
            mockDynamicLine.SetupGet(dynamicLine => dynamicLine.OriginalLineNumber).Returns(123);
            mockFirstTrackedCoverageLine.SetupGet(trackedCoverageLine => trackedCoverageLine.Line).Returns(mockDynamicLine.Object);
            var trackedCoverageLines = new TrackedCoverageLines(new List<ITrackedCoverageLine> { mockFirstTrackedCoverageLine.Object, new Mock<ITrackedCoverageLine>().Object });
            
            var firstTrackedCoverageLineInfo = trackedCoverageLines.GetFirstTrackedCoverageLineInfo();

            Assert.That(firstTrackedCoverageLineInfo.DynamicCoberturaLine, Is.SameAs(firstDynamicCoberturaLine));
            Assert.That(firstTrackedCoverageLineInfo.OriginalLineNumber, Is.EqualTo(123));
        }
    }
}
