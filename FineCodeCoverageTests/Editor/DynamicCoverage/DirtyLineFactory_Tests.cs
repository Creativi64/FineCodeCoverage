using FineCodeCoverage.Collection.ReportGeneration;
using FineCodeCoverage.Editor.DynamicCoverage;
using FineCodeCoverage.Editor.DynamicCoverage.Common;
using FineCodeCoverage.Editor.DynamicCoverage.Dirty;
using FineCodeCoverageTests.TestHelpers;
using Microsoft.VisualStudio.Text;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;

namespace FineCodeCoverageTests.Editor.DynamicCoverage
{
    internal class DirtyLineFactory_Tests
    {
        private Mock<ITrackingLineFactory> mockTrackingLineFactory;
        private Mock<ITrackingLine> mockWrappedTrackingLine;

        public ITrackingLine Setup(IDynamicCoberturaLine dynamicCoberturaLine = null)
        {
            var trackingSpan = new Mock<ITrackingSpan>().Object;
            var textSnapshot = new Mock<ITextSnapshot>().Object;
            mockTrackingLineFactory = new Mock<ITrackingLineFactory>();
            mockWrappedTrackingLine = new Mock<ITrackingLine>();
            mockTrackingLineFactory.Setup(trackingLineFactory => trackingLineFactory.Create(trackingSpan, 123, DynamicCoverageType.Dirty))
                .Returns(mockWrappedTrackingLine.Object);
            var dirtyLineFactory = new DirtyLineFactory(mockTrackingLineFactory.Object);
            return dirtyLineFactory.Create(trackingSpan, 123, dynamicCoberturaLine);
        }

        [Test]
        public void Should_Wrap_A_Dirty_TrackingLine()
        {
            var dirtyLine = Setup();

            var dynamicLine = new Mock<IDynamicLine>().Object;
            mockWrappedTrackingLine.SetupGet(wrappedTrackingLine => wrappedTrackingLine.Line).Returns(dynamicLine);

            var textSnapshot = new Mock<ITextSnapshot>().Object;
            var updatedLineNumbers = new List<int> { 0, 1 };
            mockWrappedTrackingLine.Setup(wrappedTrackingLine => wrappedTrackingLine.GetUpdatedLineNumbers(textSnapshot))
                .Returns(updatedLineNumbers);

            Assert.That(dirtyLine.Line, Is.SameAs(dynamicLine));
            Assert.That(updatedLineNumbers, Is.SameAs(dirtyLine.GetUpdatedLineNumbers(textSnapshot)));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_DynamicCoberturaLine_LineMoved_From_Wrapped_DynamicLine_When_Wrapped_Has_Updated_Line_Numbers(
            bool hasUpdatedLineNumbers    
        )
        {
            var mockDynamicCoberturaLine = new Mock<IDynamicCoberturaLine>();
            var dirtyLine = Setup(mockDynamicCoberturaLine.Object);

            var mockDynamicLine = new Mock<IDynamicLine>();
            mockDynamicLine.SetupGet(dynamicLine => dynamicLine.LineNumber).Returns(123);
            mockWrappedTrackingLine.SetupGet(wrappedTrackingLine => wrappedTrackingLine.Line).Returns(mockDynamicLine.Object);

            var textSnapshot = new Mock<ITextSnapshot>().Object;
            var updatedLineNumbers = hasUpdatedLineNumbers ? new List<int> { 0, 1 } : new List<int>();
            mockWrappedTrackingLine.Setup(wrappedTrackingLine => wrappedTrackingLine.GetUpdatedLineNumbers(textSnapshot))
                .Returns(updatedLineNumbers);

           dirtyLine.GetUpdatedLineNumbers(textSnapshot);

            mockDynamicCoberturaLine.Verify(dynamicCoberturaLine => dynamicCoberturaLine.LineMoved(123), MoqAssertionsHelper.ExpectedTimes(hasUpdatedLineNumbers));
        }
    }
}
