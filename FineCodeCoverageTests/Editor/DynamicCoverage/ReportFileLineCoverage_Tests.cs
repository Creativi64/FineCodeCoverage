using FineCodeCoverage.Collection.ReportGeneration;
using FineCodeCoverage.Editor.DynamicCoverage.Management;
using FineCodeCoverage.Editor.DynamicCoverage.Utilities;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace FineCodeCoverageTests.Editor.DynamicCoverage
{
    internal class FileLines_Tests
    {
        [Test]
        public void Should_Have_Lines()
        {
            var lines = new List<ICoberturaLine>();
            var fileLines = new FileLines(lines, new Mock<IDateTimeService>().Object);

            Assert.That(fileLines.Lines, Is.SameAs(lines));
        }

        [Test]
        public void Should_Not_Have_TrackedLines_If_No_Set()
        {
            Assert.That(new FileLines(new List<ICoberturaLine>(), new Mock<IDateTimeService>().Object).HasTrackedLines, Is.False);
        }

        [Test]
        public void Should_Have_TrackedLines_When_Set()
        {
            var fileLines = new FileLines(new List<ICoberturaLine>(), new Mock<IDateTimeService>().Object);
            fileLines.SetTrackedLines(new Mock<ITrackedLines>().Object);

            Assert.That(fileLines.HasTrackedLines, Is.True);
        }

        [Test]
        public void Should_Return_TrackedLines_If_Not_Out_Of_Date()
        {
            DateTime notOutOfDateDate = DateTime.Now;
            var mockDateTimeService = new Mock<IDateTimeService>();
            mockDateTimeService.SetupGet(dateTimeService => dateTimeService.Now).Returns(notOutOfDateDate.AddMinutes(10));
            var fileLines = new FileLines(new List<ICoberturaLine>(), mockDateTimeService.Object);
            var trackedLines = new Mock<ITrackedLines>().Object;
            fileLines.SetTrackedLines(trackedLines);

            Assert.That(trackedLines, Is.SameAs(fileLines.GetTrackedLinesIfNotOutOfDate(notOutOfDateDate)));
        }

        [Test]
        public void Should_Return_Null_TrackedLines_When_Out_Of_Date()
        {
            DateTime outOfDateDate = DateTime.Now;
            var mockDateTimeService = new Mock<IDateTimeService>();
            mockDateTimeService.SetupGet(dateTimeService => dateTimeService.Now).Returns(outOfDateDate.AddMinutes(-10));
            var fileLines = new FileLines(new List<ICoberturaLine>(), new Mock<IDateTimeService>().Object);
            var trackedLines = new Mock<ITrackedLines>().Object;
            fileLines.SetTrackedLines(trackedLines);

            Assert.That(fileLines.GetTrackedLinesIfNotOutOfDate(outOfDateDate), Is.Null);
        }

        [Test]
        public void Should_Not_Throw_When_TextView_Closed_And_No_TrackedLines()
        {
            var fileLines = new FileLines(new List<ICoberturaLine>(), new Mock<IDateTimeService>().Object);
            fileLines.TextViewClosed();
        }

        [Test]
        public void Should_Use_TextView_Closed_For_Out_Of_Date()
        {
            var lastWriteTime = DateTime.Now;

            var mockDateTimeService = new Mock<IDateTimeService>();
            mockDateTimeService.SetupSequence(dateTimeService => dateTimeService.Now)
                .Returns(lastWriteTime.AddMinutes(-10))
                .Returns(lastWriteTime.AddMinutes(10));
            var fileLines = new FileLines(new List<ICoberturaLine>(), mockDateTimeService.Object);
            var trackedLines = new Mock<ITrackedLines>().Object;
            fileLines.SetTrackedLines(trackedLines);
            
            Assert.That(fileLines.GetTrackedLinesIfNotOutOfDate(lastWriteTime), Is.Null);
            fileLines.TextViewClosed();
            Assert.That(fileLines.GetTrackedLinesIfNotOutOfDate(lastWriteTime), Is.Not.Null);
        }
    }
}
