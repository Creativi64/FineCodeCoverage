using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Editor.DynamicCoverage;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using Moq;
using NUnit.Framework;

namespace FineCodeCoverageTests.Editor.DynamicCoverage.BufferLineCoverageTests
{
    internal class BufferLineCoverage_Applicable_ContentType_Tests
    {
        [Test]
        public void Should_Not_Have_TrackedLines_If_Content_Type_Changes_To_Not_Applicable()
        {
            var setup = SetupForCoverageLines.Setup();
            var mockCoverageContentTypes = setup.AutoMoqer.GetMock<ICoverageContentTypes>();
            mockCoverageContentTypes.Setup(coverageContentTypes => coverageContentTypes.IsApplicable("AfterContentTypeTypeName"))
                .Returns(false);
            var bufferLineCoverage = setup.BufferLineCoverage;

            bufferLineCoverage.Handle(setup.NewCoverageLinesMessage);

            var mockAfterContentType = new Mock<IContentType>();
            mockAfterContentType.SetupGet(contentType => contentType.TypeName).Returns("AfterContentTypeTypeName");
            setup.TextInfoMocks.TextBuffer.Raise(textBuffer => textBuffer.ContentTypeChanged += null, 
                new ContentTypeChangedEventArgs(
                    new Mock<ITextSnapshot>().Object, 
                    new Mock<ITextSnapshot>().Object, 
                    new Mock<IContentType>().Object, 
                    mockAfterContentType.Object,
                    null));

            mockCoverageContentTypes.VerifyAll();
            Assert.That(bufferLineCoverage.HasCoverage, Is.False);
            setup.AutoMoqer.Verify<IEventAggregator>(ea => ea.SendMessage(new CoverageChangedMessage(FilePath.Value, null), null));
        }
    }
}
