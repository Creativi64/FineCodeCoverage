using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AutoMoq;
using FineCodeCoverage.Collection.CoverageProjectManagement;
using FineCodeCoverage.Collection.CoverageToolOutput;
using FineCodeCoverage.Utilities.Events;
using FineCodeCoverage.Utilities.MEF;
using FineCodeCoverage.Utilities.Wrappers;
using FineCodeCoverage.VSAbstractions.OutputWindow;
using Moq;
using NUnit.Framework;

namespace FineCodeCoverageTests
{
    class CoverageToolOutputManager_Tests
    {
        private AutoMoqer mocker;
        private Mock<ICoverageProject> mockProject1;
        private Mock<ICoverageProject> mockProject2;
        private List<ICoverageProject> coverageProjects;
        private List<int> callOrder;
        private const string DefaultCoverageFolder = "defaultFolder";

        [SetUp]
        public void SetUp()
        {
            mocker = new AutoMoqer();
            mockProject1 = new Mock<ICoverageProject>();
            mockProject1.Setup(p => p.FCCOutputFolder).Returns("p1output");
            mockProject1.Setup(p => p.ProjectName).Returns("project1");
            mockProject1.SetupProperty(p => p.CoverageOutputFolder);
            mockProject1.Setup(p => p.DefaultCoverageOutputFolder).Returns(DefaultCoverageFolder);
            mockProject2 = new Mock<ICoverageProject>();
            mockProject2.Setup(p => p.FCCOutputFolder).Returns("p2output");
            mockProject2.Setup(p => p.ProjectName).Returns("project2");
            mockProject2.Setup(p => p.DefaultCoverageOutputFolder).Returns(DefaultCoverageFolder);
            coverageProjects = new List<ICoverageProject> { mockProject1.Object, mockProject2.Object };
        }
        
        private void SetUpProviders(bool provider1First,string provider1Provides, string provider2Provides)
        {
            callOrder = new List<int>();
            var mockOrderMetadata1 = new Mock<IOrderMetadata>();
            mockOrderMetadata1.Setup(o => o.Order).Returns(provider1First? 1 : 2);
            var mockOrderMetadata2 = new Mock<IOrderMetadata>();
            mockOrderMetadata2.Setup(o => o.Order).Returns(provider1First ? 2 : 1);

            var mockCoverageToolOutputFolderProvider1 = new Mock<ICoverageToolOutputFolderProvider>();
            mockCoverageToolOutputFolderProvider1.Setup(p => p.Provide(coverageProjects)).Returns(provider1Provides).Callback(() => callOrder.Add(1));
            var mockCoverageToolOutputFolderProvider2 = new Mock<ICoverageToolOutputFolderProvider>();
            mockCoverageToolOutputFolderProvider2.Setup(p => p.Provide(coverageProjects)).Returns(provider2Provides).Callback(() => callOrder.Add(2));
            List<Lazy<ICoverageToolOutputFolderProvider, IOrderMetadata>> lazyOrderedProviders = new List<Lazy<ICoverageToolOutputFolderProvider, IOrderMetadata>>
            {
                new Lazy<ICoverageToolOutputFolderProvider, IOrderMetadata>(()=>mockCoverageToolOutputFolderProvider1.Object,mockOrderMetadata1.Object),
                new Lazy<ICoverageToolOutputFolderProvider, IOrderMetadata>(()=>mockCoverageToolOutputFolderProvider2.Object,mockOrderMetadata2.Object)
            };
            mocker.SetInstance<IEnumerable<Lazy<ICoverageToolOutputFolderProvider, IOrderMetadata>>>(lazyOrderedProviders);
        }

        [TestCase(true,1, 2)]
        [TestCase(false, 2, 1)]
        public async Task Should_Use_Providers_In_Order_When_Determining_CoverageProject_Output_Folder_Async(bool provider1First, int expectedFirst, int expectedSecond)
        {
            SetUpProviders(provider1First, null, null);
            var coverageToolOutputManager = mocker.Create<CoverageToolOutputManager>();
            await coverageToolOutputManager.SetProjectCoverageOutputFolderAsync(coverageProjects);
            Assert.AreEqual(callOrder, new List<int> { expectedFirst, expectedSecond });
        }

        [Test]
        public async Task Should_Stop_Asking_Providers_When_One_Provides_Value_Async()
        {
            SetUpProviders(true, "_", "_");
            var coverageToolOutputManager = mocker.Create<CoverageToolOutputManager>();
            await coverageToolOutputManager.SetProjectCoverageOutputFolderAsync(coverageProjects);
            Assert.AreEqual(callOrder, new List<int> { 1 });
        }

        [Test]
        public async Task Should_Try_Empty_Provided_Output_Folder_Async()
        {
            SetUpProviders(true, "Provided", "_");
            var coverageToolOutputManager = mocker.Create<CoverageToolOutputManager>();
            await coverageToolOutputManager.SetProjectCoverageOutputFolderAsync(coverageProjects);
            mocker.Verify<IFileUtil>(f => f.TryEmptyDirectory("Provided"));
        }


        [Test]
        public async Task Should_Log_When_Provided_Async()
        {
            SetUpProviders(true, "Provided", "_");
            var coverageToolOutputManager = mocker.Create<CoverageToolOutputManager>();
            await coverageToolOutputManager.SetProjectCoverageOutputFolderAsync(coverageProjects);
#pragma warning disable VSTHRD110 // Observe result of async calls
            mocker.Verify<ILogger>(l => l.LogAsync("FCC output in Provided"));
#pragma warning restore VSTHRD110 // Observe result of async calls
        }

        [Test]
        public async Task Should_Raise_The_OutdatedOutputMessage_Async()
        {
            SetUpProviders(true, "Provided", "_");
            var coverageToolOutputManager = mocker.Create<CoverageToolOutputManager>();
            await coverageToolOutputManager.SetProjectCoverageOutputFolderAsync(coverageProjects);
            mocker.Verify<IEventAggregator>(eventAggregator => eventAggregator.SendMessage(It.IsAny<OutdatedOutputMessage>(), null));
        }

        [Test]
        public async Task Should_Set_CoverageOutputFolder_To_ProjectName_Sub_Folder_Of_Provided_Async()
        {
            SetUpProviders(true, "Provided", "_");
            var coverageToolOutputManager = mocker.Create<CoverageToolOutputManager>();
            await coverageToolOutputManager.SetProjectCoverageOutputFolderAsync(coverageProjects);

            var expectedProject1OutputFolder = Path.Combine("Provided", mockProject1.Object.ProjectName);
            var expectedProject2OutputFolder = Path.Combine("Provided", mockProject2.Object.ProjectName);
            mockProject1.VerifySet(p => p.CoverageOutputFolder = expectedProject1OutputFolder);
            mockProject2.VerifySet(p => p.CoverageOutputFolder = expectedProject2OutputFolder);

        }

        [Test]
        public async Task Should_Set_CoverageOutputFolder_To_Default_For_All_When_Not_Provided_Async()
        {
            SetUpProviders(true, null, null);
            var coverageToolOutputManager = mocker.Create<CoverageToolOutputManager>();
            await coverageToolOutputManager.SetProjectCoverageOutputFolderAsync(coverageProjects);

            
            mockProject1.VerifySet(p => p.CoverageOutputFolder = DefaultCoverageFolder);
            mockProject2.VerifySet(p => p.CoverageOutputFolder = DefaultCoverageFolder);
        }
    
        [Test]
        public async Task Should_Output_Reports_To_First_Project_CoverageOutputFolder_When_Not_Provided_Async()
        {
            SetUpProviders(true, null, null);
            var coverageToolOutputManager = mocker.Create<CoverageToolOutputManager>();
            await coverageToolOutputManager.SetProjectCoverageOutputFolderAsync(coverageProjects);

            var firstProjectOutputFolder = mockProject1.Object.CoverageOutputFolder;
            
            Assert.AreEqual(coverageToolOutputManager.GetReportOutputFolder(), firstProjectOutputFolder);
        }

        [Test]
        public async Task Should_Output_Reports_To_Provided_When_Provided_Async()
        {
            SetUpProviders(true, "Provided", null);
            var coverageToolOutputManager = mocker.Create<CoverageToolOutputManager>();
            await coverageToolOutputManager.SetProjectCoverageOutputFolderAsync(coverageProjects);

            var outputFolder = coverageToolOutputManager.GetReportOutputFolder();

            Assert.AreEqual(outputFolder, "Provided");

        }

    }
}
