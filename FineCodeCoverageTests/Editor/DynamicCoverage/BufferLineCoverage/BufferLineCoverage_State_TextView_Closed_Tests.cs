using NUnit.Framework;
using System;

namespace FineCodeCoverageTests.Editor.DynamicCoverage.BufferLineCoverageTests
{
    internal class BufferLineCoverage_State_TextView_Closed_Tests {
        [TestCase(true)]
        [TestCase(false)]
        public void Should_SaveSerializedCoverage_When_TextView_Closed_And_Tracking_And_File_System_Reflecting_TrackedLines(
            bool fileSystemReflectsTrackedLines
        )
        {
            throw new NotImplementedException();
            // SimpleSetUp(false);
            // var bufferLineCoverage = autoMoqer.Create<BufferLineCoverage>();

            // var snapshotText = "snapshot text";
            // var trackedLines = new Mock<ITrackedLines>().Object;
            // autoMoqer.Setup<ITrackedLinesFactory, string>(
            //     trackedLinesFactory => trackedLinesFactory.Serialize(trackedLines, snapshotText)
            // ).Returns("serialized");
            // autoMoqer.Setup<ITrackedLinesFactory, ITrackedLines>(
            //    trackedLinesFactory => trackedLinesFactory.Create(It.IsAny<List<ICoberturaLine>>(),It.IsAny<ITextSnapshot>(),It.IsAny<string>())
            //).Returns(trackedLines);
            // bufferLineCoverage.Handle(new NewCoverageLinesMessage(new Mock<IFileLineCoverage>().Object));


            // mockTextInfo.Setup(textInfo => textInfo.GetFileText())
            //     .Returns(fileSystemReflectsTrackedLines ? snapshotText : "changes not saved");

            // var mockTextViewCurrentSnapshot = new Mock<ITextSnapshot>();
            // mockTextViewCurrentSnapshot.Setup(snapshot => snapshot.GetText()).Returns(snapshotText);
            // mockTextView.Setup(textView => textView.TextSnapshot).Returns(mockTextViewCurrentSnapshot.Object);

            // mockTextView.Raise(textView => textView.Closed += null, EventArgs.Empty);

            // autoMoqer.Verify<IDynamicCoverageStore>(
            //     dynamicCoverageStore => dynamicCoverageStore.SaveSerializedCoverage(filePath, "serialized"), 
            //     fileSystemReflectsTrackedLines ? Times.Once() : Times.Never());

        }

        [Test]
        public void Should_Remove_Serialized_Coverage_When_TextView_Closed_And_No_TrackedLines()
        {
            throw new NotImplementedException();
            //SimpleSetUp(false);
            //var bufferLineCoverage = autoMoqer.Create<BufferLineCoverage>();

            //mockTextView.Raise(textView => textView.Closed += null, EventArgs.Empty);

            //autoMoqer.Verify<IDynamicCoverageStore>(dynamicCoverageStore => dynamicCoverageStore.RemoveSerializedCoverage(filePath));
        }
    }
}
