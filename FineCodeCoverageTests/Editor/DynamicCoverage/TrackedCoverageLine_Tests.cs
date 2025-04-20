using System.Collections.Generic;
using System.Linq;
using FineCodeCoverage.Editor.DynamicCoverage;
using FineCodeCoverage.Engine.ReportGenerator;
using Microsoft.VisualStudio.Text;
using Moq;
using NUnit.Framework;

namespace FineCodeCoverageTests.Editor.DynamicCoverage
{
    internal class TrackedCoverageLine_Tests
    {
        [TestCase(CoverageType.Covered, DynamicCoverageType.Covered)]
        [TestCase(CoverageType.NotCovered, DynamicCoverageType.NotCovered)]
        [TestCase(CoverageType.Partial, DynamicCoverageType.Partial)]
        public void Should_Have_A_DynamicLine_From_ICoberturaLine_When_Constructed(CoverageType lineCoverageType, DynamicCoverageType expectedDynamicCoverageType)
        {
            var mockLine = new Mock<ICoberturaLine>();
            mockLine.SetupGet(l => l.CoverageType).Returns(lineCoverageType);
            mockLine.SetupGet(l => l.Number).Returns(1);

            var coverageLine = new TrackedCoverageLine(null, mockLine.Object, null);

            Assert.That(coverageLine.Line.CoverageType, Is.EqualTo(expectedDynamicCoverageType));
            Assert.That(coverageLine.Line.LineNumber, Is.EqualTo(0));
            Assert.That(coverageLine.Line.OriginalLineNumber, Is.EqualTo(0));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Be_Updated_If_The_Line_Number_Changes(bool updateLineNumber)
        {
            var mockLine = new Mock<ICoberturaLine>();
            mockLine.SetupGet(l => l.Number).Returns(1);

            var currentTextSnapshot = new Mock<ITextSnapshot>().Object;
            var trackingSpan = new Mock<ITrackingSpan>().Object;
            var mockLineTracker = new Mock<ILineTracker>();

            var updatedLineNumber = updateLineNumber ? 10 : 0;
            mockLineTracker.Setup(lineTracker => lineTracker.GetLineNumber(trackingSpan, currentTextSnapshot, true))
                .Returns(updatedLineNumber);
            var coverageLine = new TrackedCoverageLine(trackingSpan, mockLine.Object, mockLineTracker.Object);

            var updatedLineNumbers = coverageLine.GetUpdateLineNumbers(currentTextSnapshot);

            Assert.That(updatedLineNumbers, Is.EqualTo(updateLineNumber ? new List<int> { 0, 10 } : Enumerable.Empty<int>()));

            Assert.That(coverageLine.Line.LineNumber, Is.EqualTo(updatedLineNumber));
        }
    
        [Test]
        public void Should_Update_IDynamicCoberturaLine_If_Is_That_Type()
        {
            var mockDynamicCoberturaLine = new Mock<IDynamicCoberturaLine>();
            mockDynamicCoberturaLine.SetupGet(l => l.Number).Returns(1);

            var currentTextSnapshot = new Mock<ITextSnapshot>().Object;
            var trackingSpan = new Mock<ITrackingSpan>().Object;
            var mockLineTracker = new Mock<ILineTracker>();

            mockLineTracker.Setup(lineTracker => lineTracker.GetLineNumber(trackingSpan, currentTextSnapshot, true))
                .Returns(10);
            var coverageLine = new TrackedCoverageLine(trackingSpan, mockDynamicCoberturaLine.Object, mockLineTracker.Object);

            coverageLine.GetUpdateLineNumbers(currentTextSnapshot);

            mockDynamicCoberturaLine.Verify(dynamicCoberturaLine => dynamicCoberturaLine.LineMoved(10));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Have_DynamicCoberturaLine_If_is_That_Type(bool isDynamicCoberturaLine)
        {
            var trackedCoverageLines = new TrackedCoverageLine(
                new Mock<ITrackingSpan>().Object, 
                isDynamicCoberturaLine ? new Mock<IDynamicCoberturaLine>().Object : new Mock<ICoberturaLine>().Object, 
                new Mock<ILineTracker>().Object);

            Assert.That(trackedCoverageLines.DynamicCoberturaLine != null, Is.EqualTo(isDynamicCoberturaLine));
        }
    }
}
