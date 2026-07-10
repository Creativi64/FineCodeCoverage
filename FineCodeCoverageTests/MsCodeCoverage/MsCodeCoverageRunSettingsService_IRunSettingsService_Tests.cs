using Moq;
using NUnit.Framework;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using System;
using System.IO;
using System.Xml.XPath;
using System.Collections.Generic;
using AutoMoq;
using System.Threading;
using FineCodeCoverage.Collection.Ms;
using System.Threading.Tasks;
using FineCodeCoverage.Initialization.ToolZip;
using ILogger = FineCodeCoverage.VSAbstractions.OutputWindow.ILogger;

namespace FineCodeCoverageTests.MsCodeCoverage
{
    internal class MsCodeCoverageRunSettingsService_IRunSettingsService_Tests
    {
        private AutoMoqer autoMocker;
        private MsCodeCoverageRunSettingsService msCodeCoverageRunSettingsService;
        [SetUp]
        public void CreateSut()
        {
            autoMocker = new AutoMoqer();
            msCodeCoverageRunSettingsService = autoMocker.Create<MsCodeCoverageRunSettingsService>();
        }

        [Test]
        public void Should_Have_A_Name()
        {
            Assert.False(string.IsNullOrWhiteSpace(msCodeCoverageRunSettingsService.Name));
        }

        [TestCase(RunSettingConfigurationInfoState.Discovery)]
        [TestCase(RunSettingConfigurationInfoState.None)]
        public void Should_Not_Delegate_To_UserRunSettingsService_When_Not_Test_Execution(RunSettingConfigurationInfoState state)
        {
            SetuserRunSettingsProjectDetailsLookup(false);
            msCodeCoverageRunSettingsService.CollectionStatus = MsCodeCoverageCollectionStatus.Collecting;

            ShouldNotDelegateToUserRunSettingsService(state);
        }

        [TestCase(MsCodeCoverageCollectionStatus.NotCollecting)]
        [TestCase(MsCodeCoverageCollectionStatus.Error)]
        public void Should_Not_Delegate_To_UserRunSettingsService_When_Is_Not_Collecting(MsCodeCoverageCollectionStatus status)
        {
            msCodeCoverageRunSettingsService.CollectionStatus = status;
            SetuserRunSettingsProjectDetailsLookup(false);
            
            ShouldNotDelegateToUserRunSettingsService(RunSettingConfigurationInfoState.Execution);
        }

        [Test]
        public void Should_Not_Delegate_To_UserRunSettingsService_When_No_User_RunSettings()
        {
            msCodeCoverageRunSettingsService.CollectionStatus = MsCodeCoverageCollectionStatus.Collecting;
            SetuserRunSettingsProjectDetailsLookup(true);

            ShouldNotDelegateToUserRunSettingsService(RunSettingConfigurationInfoState.Execution);
        }

        private void ShouldNotDelegateToUserRunSettingsService(RunSettingConfigurationInfoState state)
        {
            var mockRunSettingsConfigurationInfo = new Mock<IRunSettingsConfigurationInfo>();
            mockRunSettingsConfigurationInfo.Setup(ci => ci.RequestState).Returns(state);

            autoMocker.GetMock<IUserRunSettingsService>()
                .Setup(userRunSettingsService => userRunSettingsService.AddFCCRunSettings(
                    It.IsAny<IXPathNavigable>(),
                    It.IsAny<IRunSettingsConfigurationInfo>(),
                    It.IsAny<Dictionary<string, IUserRunSettingsProjectDetails>>(),
                    It.IsAny<string>()
                )).Returns(new Mock<IXPathNavigable>().Object);
            Assert.IsNull(msCodeCoverageRunSettingsService.AddRunSettings(null, mockRunSettingsConfigurationInfo.Object, null));
        }



        private void SetuserRunSettingsProjectDetailsLookup(bool empty)
        {
            var userRunSettingsProjectDetailsLookup = new Dictionary<string, IUserRunSettingsProjectDetails>();
            if (!empty)
            {
                userRunSettingsProjectDetailsLookup.Add("", null); // an entry
            }
            msCodeCoverageRunSettingsService.UserRunSettingsProjectDetailsLookup = userRunSettingsProjectDetailsLookup;
        }

        [Test]
        public async Task Should_Delegate_To_UserRunSettingsService_With_UserRunSettingsProjectDetailsLookup_And_FCC_Ms_TestAdapter_Path_When_Applicable_Async()
        {
            var inputRunSettingDocument = new Mock<IXPathNavigable>().Object;

            var mockRunSettingsConfigurationInfo = new Mock<IRunSettingsConfigurationInfo>();
            mockRunSettingsConfigurationInfo.Setup(ci => ci.RequestState).Returns(RunSettingConfigurationInfoState.Execution);
            var runSettingsConfigurationInfo = mockRunSettingsConfigurationInfo.Object;

            var fccMsTestAdapter = await GetFCCMsTestAdapterPathAsync();

            // IsCollecting would set this
            var userRunSettingsProjectDetailsLookup = new Dictionary<string, IUserRunSettingsProjectDetails>
            {
                { "",null} // an entry
            };
            msCodeCoverageRunSettingsService.UserRunSettingsProjectDetailsLookup = userRunSettingsProjectDetailsLookup;
            msCodeCoverageRunSettingsService.CollectionStatus = MsCodeCoverageCollectionStatus.Collecting;


            var mockUserRunSettingsService = autoMocker.GetMock<IUserRunSettingsService>();
            var fccRunSettingDocument = new Mock<IXPathNavigable>().Object;
            var addFCCRunSettingsSetup = mockUserRunSettingsService.Setup(userRunSettingsService => userRunSettingsService.AddFCCRunSettings(
                inputRunSettingDocument, 
                runSettingsConfigurationInfo,
                It.IsAny<Dictionary<string,IUserRunSettingsProjectDetails>>(), 
                fccMsTestAdapter)
            ).Returns(fccRunSettingDocument);

            Assert.AreSame(fccRunSettingDocument, msCodeCoverageRunSettingsService.AddRunSettings(inputRunSettingDocument, mockRunSettingsConfigurationInfo.Object, null));

            var addFCCRunSettingsInvocation = mockUserRunSettingsService.Invocations[0];
            Assert.AreSame(userRunSettingsProjectDetailsLookup, addFCCRunSettingsInvocation.Arguments[2]);
        }

        [Test]
        public void Should_Return_Null_And_Log_When_Adding_FCC_RunSettings_Throws()
        {
            msCodeCoverageRunSettingsService.CollectionStatus = MsCodeCoverageCollectionStatus.Collecting;
            SetuserRunSettingsProjectDetailsLookup(false);

            var mockRunSettingsConfigurationInfo = new Mock<IRunSettingsConfigurationInfo>();
            mockRunSettingsConfigurationInfo.Setup(ci => ci.RequestState).Returns(RunSettingConfigurationInfoState.Execution);

            var exception = new Exception("add fcc runsettings exception");
            autoMocker.GetMock<IUserRunSettingsService>()
                .Setup(userRunSettingsService => userRunSettingsService.AddFCCRunSettings(
                    It.IsAny<IXPathNavigable>(),
                    It.IsAny<IRunSettingsConfigurationInfo>(),
                    It.IsAny<Dictionary<string, IUserRunSettingsProjectDetails>>(),
                    It.IsAny<string>()
                )).Throws(exception);

            Assert.IsNull(msCodeCoverageRunSettingsService.AddRunSettings(null, mockRunSettingsConfigurationInfo.Object, null));

            autoMocker.Verify<ILogger>(logger => logger.LogFileAndForget(
                "Ms code coverage - exception adding fcc runsettings to the test execution. There will be no coverage for this run.",
                exception.ToString()));
        }

        [Test]
        public void Should_Not_Delegate_To_UserRunSettingsService_When_No_Test_Execution_Containers_Are_In_The_Lookup()
        {
            msCodeCoverageRunSettingsService.CollectionStatus = MsCodeCoverageCollectionStatus.Collecting;
            msCodeCoverageRunSettingsService.UserRunSettingsProjectDetailsLookup = new Dictionary<string, IUserRunSettingsProjectDetails>
            {
                { "InLookup.dll", null },
            };

            var mockRunSettingsConfigurationInfo = CreateExecutionConfigurationInfo("NotInLookup.dll");

            Assert.IsNull(msCodeCoverageRunSettingsService.AddRunSettings(null, mockRunSettingsConfigurationInfo.Object, null));

            autoMocker.Verify<IUserRunSettingsService>(userRunSettingsService => userRunSettingsService.AddFCCRunSettings(
                It.IsAny<IXPathNavigable>(),
                It.IsAny<IRunSettingsConfigurationInfo>(),
                It.IsAny<Dictionary<string, IUserRunSettingsProjectDetails>>(),
                It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void Should_Delegate_To_UserRunSettingsService_And_Count_When_Some_Test_Execution_Containers_Are_In_The_Lookup()
        {
            msCodeCoverageRunSettingsService.CollectionStatus = MsCodeCoverageCollectionStatus.Collecting;
            msCodeCoverageRunSettingsService.UserRunSettingsProjectDetailsLookup = new Dictionary<string, IUserRunSettingsProjectDetails>
            {
                { "InLookup.dll", null },
            };

            var mockRunSettingsConfigurationInfo = CreateExecutionConfigurationInfo("InLookup.dll", "NotInLookup.dll");

            _ = msCodeCoverageRunSettingsService.AddRunSettings(null, mockRunSettingsConfigurationInfo.Object, null);

            autoMocker.Verify<IUserRunSettingsService>(userRunSettingsService => userRunSettingsService.AddFCCRunSettings(
                It.IsAny<IXPathNavigable>(),
                It.IsAny<IRunSettingsConfigurationInfo>(),
                It.IsAny<Dictionary<string, IUserRunSettingsProjectDetails>>(),
                It.IsAny<string>()), Times.Once());
            Assert.AreEqual(1, msCodeCoverageRunSettingsService.AddedFCCRunSettingsCount);
        }

        private static Mock<IRunSettingsConfigurationInfo> CreateExecutionConfigurationInfo(params string[] containerSources)
        {
            var mockRunSettingsConfigurationInfo = new Mock<IRunSettingsConfigurationInfo>();
            mockRunSettingsConfigurationInfo.Setup(ci => ci.RequestState).Returns(RunSettingConfigurationInfoState.Execution);
            var testContainers = new List<ITestContainer>();
            foreach (var containerSource in containerSources)
            {
                var mockTestContainer = new Mock<ITestContainer>();
                mockTestContainer.Setup(tc => tc.Source).Returns(containerSource);
                testContainers.Add(mockTestContainer.Object);
            }

            mockRunSettingsConfigurationInfo.Setup(ci => ci.TestContainers).Returns(testContainers);
            return mockRunSettingsConfigurationInfo;
        }

        private async Task<string> GetFCCMsTestAdapterPathAsync()
        {
            autoMocker.GetMock<IToolUnzipper>()
                .Setup(
                    toolFolder => toolFolder.EnsureUnzipped(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>())
                )
                .Returns("ZipDestination");

            await msCodeCoverageRunSettingsService.InitializeAsync(null, CancellationToken.None);
            return Path.Combine("ZipDestination", "build", "netstandard2.0");
        }
    
    }
}
