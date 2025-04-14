using System;
using System.Collections.Generic;
using AutoMoq;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Editor.DynamicCoverage;
using FineCodeCoverage.Editor.DynamicCoverage.Utilities;
using FineCodeCoverage.Engine.ReportGenerator;
using FineCodeCoverage.Impl;
using FineCodeCoverage.Output;
using FineCodeCoverageTests.TestHelpers;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using Moq;
using NUnit.Framework;

namespace FineCodeCoverageTests.Editor.DynamicCoverage
{
    internal class DynamicCoverageManager_Tests
    {
        [Test]
        public void Should_Export_IInitializable()
        {
            ExportsInitializable.Should_Export_IInitializable<DynamicCoverageManager>();
        }

        [Test]
        public void Should_Add_Itself_As_A_Listener()
        {
            var autoMocker = new AutoMoqer();
            var dynamicCoverageManager = autoMocker.Create<DynamicCoverageManager>();

            autoMocker.Verify<IEventAggregator>(e => e.AddListener(dynamicCoverageManager, null), Times.Once());
        }

        [Test]
        public void Should_Send_NewCoverageLinesMessage_With_ReportFileLineCoverage_When_Handle_NewReportMessage()
        {
            var autoMocker = new AutoMoqer();
            var mockReportResult = new Mock<IDynamicReportResult>();
            IReadOnlyList<IAssembly> assemblies = new List<IAssembly> { new Mock<IAssembly>().Object };
            mockReportResult.SetupGet(r => r.Assemblies).Returns(assemblies);
            IFileLineCoverage flc = new Mock<IFileLineCoverage>().Object;
            autoMocker.Setup<IReportFileLineCoverageFactory, IFileLineCoverage>(f => f.Create(assemblies))
                .Returns(flc);
            
            var dynamicCoverageManager = autoMocker.Create<DynamicCoverageManager>();
            
            dynamicCoverageManager.Handle(new NewReportMessage(mockReportResult.Object, null));
            
            autoMocker.Verify<IEventAggregator>(ea => ea.SendMessage(It.Is<NewCoverageLinesMessage>(msg => msg.CoverageLines == flc), null));
        }

        [Test]
        public void Manage_Should_Create_Singleton_IBufferLineCoverage()
        {
            var autoMocker = new AutoMoqer();
            var dynamicCoverageManager = autoMocker.Create<DynamicCoverageManager>();

            var mockTextInfo = new Mock<ITextInfo>();
            var previousBufferLineCoverage = new Mock<IBufferLineCoverage>().Object;
            var propertyCollection = new PropertyCollection();
            propertyCollection.GetOrCreateSingletonProperty(() => previousBufferLineCoverage);
            mockTextInfo.Setup(textInfo => textInfo.TextBuffer.Properties).Returns(propertyCollection);
            
            var bufferLineCoverage = dynamicCoverageManager.Manage(mockTextInfo.Object);
            
            Assert.That(bufferLineCoverage, Is.SameAs(previousBufferLineCoverage));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Manage_Should_Create_Singleton_IBufferLineCoverage_With_Last_Coverage_And_Dependencies(bool hasLastCoverage)
        {
            var autoMocker = new AutoMoqer();

            var now = new DateTime();
            autoMocker.GetMock<IDateTimeService>().Setup(dateTimeService => dateTimeService.Now).Returns(now);

            var eventAggregator = autoMocker.GetMock<IEventAggregator>().Object;
            var trackedLinesFactory = autoMocker.GetMock<ITrackedLinesFactory>().Object;
            var dynamicCoverageManager = autoMocker.Create<DynamicCoverageManager>();
            LastCoverage expectedLastCoverage = null;
            if (hasLastCoverage)
            {
                var fileLineCoverage = new Mock<IFileLineCoverage>().Object;
                autoMocker.Setup<IReportFileLineCoverageFactory, IFileLineCoverage>(rflcf => rflcf.Create(It.IsAny<IReadOnlyList<IAssembly>>()))
                    .Returns(fileLineCoverage);
                
                expectedLastCoverage = new LastCoverage(fileLineCoverage, now);
                (dynamicCoverageManager as IListener<TestExecutionStartingMessage>).Handle(new TestExecutionStartingMessage());
                dynamicCoverageManager.Handle(new NewReportMessage(new Mock<IReportResult>().Object,null));
            }

            var mockTextInfo = new Mock<ITextInfo>();
            var textView = new Mock<ITextView>().Object;
            var propertyCollection = new PropertyCollection();
            mockTextInfo.Setup(textInfo => textInfo.TextBuffer.Properties).Returns(propertyCollection);

            var newBufferLineCoverage = new Mock<IBufferLineCoverage>().Object;
            var mockBufferLineCoverageFactory = autoMocker.GetMock<IBufferLineCoverageFactory>();
            var mockTextDocument = new Mock<ITextDocument>();
            mockTextDocument.Setup(textDocument => textDocument.FilePath).Returns("filepath");
            mockBufferLineCoverageFactory.Setup(
                bufferLineCoverageFactory => bufferLineCoverageFactory.Create(expectedLastCoverage, mockTextInfo.Object, eventAggregator, trackedLinesFactory))
                .Returns(newBufferLineCoverage);



            var bufferLineCoverage = dynamicCoverageManager.Manage(mockTextInfo.Object);

            Assert.That(bufferLineCoverage, Is.SameAs(newBufferLineCoverage));
        }
    }
}
