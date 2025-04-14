using FineCodeCoverage.Editor.DynamicCoverage;
using NUnit.Framework;
using System;

namespace FineCodeCoverageTests.Editor.DynamicCoverage.BufferLineCoverageTests
{
    internal class BufferLineCoverage_LastCoverage_Tests
    {
        [Test]
        public void Should_Create_TrackedLines_From_Serialized_Coverage_If_Present_And_Not_Out_Of_Date()
        {
            throw new NotImplementedException();
            //SimpleSetUp(false);

            //DateTime lastWriteDate = new DateTime(2024, 5, 8);
            //DateTime textExecutionStartingDate = new DateTime(2024, 5, 10);
            //DateTime serializedDate = new DateTime(2024, 5, 8);

            //mockTextInfo.Setup(textInfo => textInfo.GetLastWriteTime()).Returns(lastWriteDate);
            //var mockLastCoverage = autoMoqer.GetMock<ILastCoverage>();
            //mockLastCoverage.SetupGet(lastCoverage => lastCoverage.TestExecutionStartingDate).Returns(textExecutionStartingDate);
            //mockLastCoverage.SetupGet(lastCoverage => lastCoverage.FileLineCoverage).Returns(new Mock<IFileLineCoverage>().Object);

            //autoMoqer.Setup<IDynamicCoverageStore, SerializedCoverageWhen>(dynamicCoverageStore => dynamicCoverageStore.GetSerializedCoverage(filePath))
            //    .Returns(new SerializedCoverageWhen { Serialized = "serialized", When = serializedDate });

            //var mockTrackedLinesFactory = autoMoqer.GetMock<ITrackedLinesFactory>();
            //mockTrackedLinesFactory.Setup(
            //    trackedLinesFactory => trackedLinesFactory.Create("serialized", textSnapshot, filePath)
            //).Returns(new Mock<ITrackedLines>().Object);

            //var bufferLineCoverage = autoMoqer.Create<BufferLineCoverage>();

            //mockTrackedLinesFactory.VerifyAll();
            //Assert.That(bufferLineCoverage.HasCoverage, Is.True);
        }

        [Test]
        public void Should_Not_Create_TrackedLines_When_Existing_Coverage_When_LastWriteTime_After_LastTestExecutionStarting_When_No_Serialized_Coverage()
        {
            throw new NotImplementedException();
            //SimpleSetUp(false);
            //mockTextInfo.Setup(textInfo => textInfo.GetLastWriteTime()).Returns(new DateTime(2024, 5, 10));
            //var mockLastCoverage = autoMoqer.GetMock<ILastCoverage>();
            //mockLastCoverage.SetupGet(lastCoverage => lastCoverage.TestExecutionStartingDate).Returns(new DateTime(2024, 5, 9));
            //mockLastCoverage.SetupGet(lastCoverage => lastCoverage.FileLineCoverage).Returns(new Mock<IFileLineCoverage>().Object);

            //var bufferLineCoverage = autoMoqer.Create<BufferLineCoverage>();

            //Assert.IsFalse(bufferLineCoverage.HasCoverage);
        }

        private BufferLineCoverage SerializedCoverageisOutOfDate()
        {
            throw new NotImplementedException();
            //SimpleSetUp(false);

            //DateTime lastWriteDate = new DateTime(2024, 5, 9);
            //DateTime textExecutionStartingDate = new DateTime(2024, 5, 10);
            //DateTime serializedDate = new DateTime(2024, 5, 8);

            //mockTextInfo.Setup(textInfo => textInfo.GetLastWriteTime()).Returns(lastWriteDate);
            //var mockLastCoverage = autoMoqer.GetMock<ILastCoverage>();
            //mockLastCoverage.SetupGet(lastCoverage => lastCoverage.TestExecutionStartingDate).Returns(textExecutionStartingDate);
            //mockLastCoverage.SetupGet(lastCoverage => lastCoverage.FileLineCoverage).Returns(new Mock<IFileLineCoverage>().Object);

            //autoMoqer.Setup<IDynamicCoverageStore, SerializedCoverageWhen>(dynamicCoverageStore => dynamicCoverageStore.GetSerializedCoverage(filePath))
            //    .Returns(new SerializedCoverageWhen { Serialized = "serialized", When = serializedDate });

            //return autoMoqer.Create<BufferLineCoverage>();
        }

        [Test]
        public void Should_Not_Create_TrackedLines_When_Existing_Coverage_When_Serialized_Coverage_Is_Out_Of_Date()
        {
            throw new NotImplementedException();
            //var bufferLineCoverage = SerializedCoverageisOutOfDate();

            //Assert.IsFalse(bufferLineCoverage.HasCoverage);
        }

        [Test]
        public void Should_Log_When_Not_Creating_TrackedLines_As_Out_Of_Date()
        {
            throw new NotImplementedException();
            //SerializedCoverageisOutOfDate();

            //autoMoqer.Verify<ILogger>(logger => logger.Log($"Not creating editor marks for {filePath} as coverage is out of date"));
        }

        [Test]
        public void Should_Create_TrackedLines_When_No_Serialized_Coverage_And_Not_Out_Of_Date()
        {
            throw new NotImplementedException();
            //SimpleSetUp(false);

            //DateTime lastWriteDate = new DateTime(2024, 5, 9);
            //DateTime textExecutionStartingDate = new DateTime(2024, 5, 10);

            //mockTextInfo.Setup(textInfo => textInfo.GetLastWriteTime()).Returns(lastWriteDate);
            //var mockLastCoverage = autoMoqer.GetMock<ILastCoverage>();
            //mockLastCoverage.SetupGet(lastCoverage => lastCoverage.TestExecutionStartingDate).Returns(textExecutionStartingDate);
            //var mockFileLineCoverage = new Mock<IFileLineCoverage>();
            //mockLastCoverage.SetupGet(lastCoverage => lastCoverage.FileLineCoverage).Returns(mockFileLineCoverage.Object);
            //var lines = new List<ICoberturaLine> { new Mock<ICoberturaLine>().Object };
            //mockFileLineCoverage.Setup(fileLineCoverage => fileLineCoverage.GetLines(filePath)).Returns(lines);
            //var trackedLinesFromSerialized = new Mock<ITrackedLines>().Object;
            //var mockTrackedLinesFactory = autoMoqer.GetMock<ITrackedLinesFactory>();
            //mockTrackedLinesFactory.Setup(
            //    trackedLinesFactory => trackedLinesFactory.Create(lines, textSnapshot, filePath)
            //).Returns(trackedLinesFromSerialized);

            //var bufferLineCoverage = autoMoqer.Create<BufferLineCoverage>();

            //mockTrackedLinesFactory.VerifyAll();
            //Assert.True(bufferLineCoverage.HasCoverage);
        }

    }
}
