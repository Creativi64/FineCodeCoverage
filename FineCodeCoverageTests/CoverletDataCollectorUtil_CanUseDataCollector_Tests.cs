using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml.Linq;
using AutoMoq;
using FineCodeCoverage.Core.Coverlet;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine.Coverlet;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverageTests.TestHelpers;
using Moq;
using NUnit.Framework;

namespace Test
{
    public class CoverletDataCollectorUtil_CanUseDataCollector_Tests
    {
        private AutoMoqer mocker;
        private CoverletDataCollectorUtil coverletDataCollectorUtil;

        public enum UseDataCollectorElement { True, False, Empty, None }

        [SetUp]
        public void SetUp()
        {
            mocker = new AutoMoqer();
            coverletDataCollectorUtil = mocker.Create<CoverletDataCollectorUtil>();
        }

        private void SetUpRunSettings(Mock<ICoverageProject> mockCoverageProject, Action<Mock<IRunSettingsCoverletConfiguration>> setup)
        {
            mockCoverageProject.Setup(p => p.RunSettingsFile).Returns(".runsettings");
            var mockRunSettingsCoverletConfiguration = mocker.GetMock<IRunSettingsCoverletConfiguration>();
            var mockRunSettingsCoverletConfigurationFactory = mocker.GetMock<IRunSettingsCoverletConfigurationFactory>();
            mockRunSettingsCoverletConfigurationFactory.Setup(rscf => rscf.Create()).Returns(mockRunSettingsCoverletConfiguration.Object);
            setup?.Invoke(mockRunSettingsCoverletConfiguration);
        }
        
        private void SetupDataCollectorState(Mock<ICoverageProject> mockCoverageProject, CoverletDataCollectorState coverletDataCollectorState)
        {
            SetUpRunSettings(mockCoverageProject, mrsc => mrsc.Setup(rsc => rsc.CoverletDataCollectorState).Returns(coverletDataCollectorState));
        }

        private XElement GetProjectElementWithDataCollector(UseDataCollectorElement useDataCollector)
        {
            var useDataCollectorElement = "";
            if(useDataCollector != UseDataCollectorElement.None)
            {
                var value = "";
                if(useDataCollector == UseDataCollectorElement.True)
                {
                    value = "true";
                }
                if(useDataCollector == UseDataCollectorElement.False)
                {
                    value = "false";
                }
                useDataCollectorElement = $"<PropertyGroup><UseDataCollector>{value}</UseDataCollector></PropertyGroup>";
            }
            
            return XElement.Parse($@"<Project>
{useDataCollectorElement}
</Project>");
        }

        

        [Test]
        public async Task Should_Factory_Create_Configuration_And_Read_CoverageProject_RunSettings_Async()
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            mockCoverageProject.Setup(cp => cp.ProjectFileXElement).Returns(GetProjectElementWithDataCollector(UseDataCollectorElement.True));
            var runSettingsFilePath = ".runsettings";
            mockCoverageProject.Setup(cp => cp.RunSettingsFile).Returns(runSettingsFilePath);

            var settingsXml = "<settings../>";
            var mockFileUtil = mocker.GetMock<IFileUtil>();
            mockFileUtil.Setup(f => f.ReadAllText(runSettingsFilePath)).Returns(settingsXml);

            var mockRunSettingsCoverletConfiguration = new Mock<IRunSettingsCoverletConfiguration>();
            var mockRunSettingsCoverletConfigurationFactory = mocker.GetMock<IRunSettingsCoverletConfigurationFactory>();
            mockRunSettingsCoverletConfigurationFactory.Setup(rscf => rscf.Create()).Returns(mockRunSettingsCoverletConfiguration.Object);
            await coverletDataCollectorUtil.CanUseDataCollectorAsync(mockCoverageProject.Object);

            mockRunSettingsCoverletConfigurationFactory.VerifyAll();
            mockRunSettingsCoverletConfiguration.Verify(rsc => rsc.Read(settingsXml));

        }

        [TestCase(UseDataCollectorElement.None)]
        [TestCase(UseDataCollectorElement.True)]
        public async Task Should_Use_Data_Collector_If_RunSettings_Has_The_Data_Collector_Enabled_And_Not_Overridden_By_Project_File_Async(UseDataCollectorElement useDataCollector)
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            mockCoverageProject.Setup(cp => cp.ProjectFileXElement).Returns(GetProjectElementWithDataCollector(useDataCollector));
            SetupDataCollectorState(mockCoverageProject, CoverletDataCollectorState.Enabled);
            
            Assert.True(await coverletDataCollectorUtil.CanUseDataCollectorAsync(mockCoverageProject.Object));
        }

        [Test]
        public async Task Should_Not_Use_Data_Collector_If_RunSettings_Has_The_Data_Collector_Enabled_And_Overridden_By_Project_File_Async()
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            mockCoverageProject.Setup(cp => cp.ProjectFileXElement).Returns(GetProjectElementWithDataCollector(UseDataCollectorElement.False));

            SetupDataCollectorState(mockCoverageProject, CoverletDataCollectorState.Enabled);

            Assert.False(await coverletDataCollectorUtil.CanUseDataCollectorAsync(mockCoverageProject.Object));
        }

        [TestCase(UseDataCollectorElement.True)]
        [TestCase(UseDataCollectorElement.Empty)]
        public async Task Should_Use_Data_Collector_If_Not_Specified_In_RunSettings_And_Specified_In_ProjectFile_Async(UseDataCollectorElement useDataCollectorElement)
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            mockCoverageProject.Setup(cp => cp.ProjectFileXElement).Returns(GetProjectElementWithDataCollector(useDataCollectorElement));

            SetupDataCollectorState(mockCoverageProject, CoverletDataCollectorState.NotPresent);

            Assert.True(await coverletDataCollectorUtil.CanUseDataCollectorAsync(mockCoverageProject.Object));
        }

        [TestCase(UseDataCollectorElement.True, true)]
        [TestCase(UseDataCollectorElement.Empty, true)]
        [TestCase(UseDataCollectorElement.False, false)]
        [TestCase(UseDataCollectorElement.None, false)]
        public async Task Should_Use_Data_Collector_If_Not_Specified_In_RunSettings_And_Specified_In_ProjectFile_VSBuild_Async(UseDataCollectorElement useDataCollector, bool expected)
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            var guid = Guid.NewGuid();
            mockCoverageProject.Setup(cp => cp.Id).Returns(guid);
            mockCoverageProject.Setup(cp => cp.ProjectFileXElement).Returns(new XElement("Root"));


            var mockVsBuildFCCSettingsProvider = mocker.GetMock<IVsBuildFCCSettingsProvider>();
            var useDataCollectorElement = "";
            if (useDataCollector != UseDataCollectorElement.None)
            {
                var value = "";
                if (useDataCollector == UseDataCollectorElement.True)
                {
                    value = "true";
                }
                if (useDataCollector == UseDataCollectorElement.False)
                {
                    value = "false";
                }
                useDataCollectorElement = $"<UseDataCollector>{value}</UseDataCollector>";
            }
            XElement vsBuildProjectFileElement = XElement.Parse($"<FineCodeCoverage>{useDataCollectorElement}</FineCodeCoverage>");
            mockVsBuildFCCSettingsProvider.Setup(
                vsBuildFCCSettingsProvider =>
                vsBuildFCCSettingsProvider.GetSettingsAsync(guid)
            ).ReturnsAsync(vsBuildProjectFileElement);
           



            Assert.AreEqual(expected,await coverletDataCollectorUtil.CanUseDataCollectorAsync(mockCoverageProject.Object));
        }

        [Test]
        public async Task Should_Use_Data_Collector_If_No_RunSettings_And_Specified_In_ProjectFile_Async()
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            mockCoverageProject.Setup(cp => cp.RunSettingsFile).Returns((string)null);
            mockCoverageProject.Setup(cp => cp.ProjectFileXElement).Returns(GetProjectElementWithDataCollector(UseDataCollectorElement.True));

            Assert.True(await coverletDataCollectorUtil.CanUseDataCollectorAsync(mockCoverageProject.Object));
        }


        [TestCase(UseDataCollectorElement.False)]
        [TestCase(UseDataCollectorElement.None)]
        public async Task Should_Not_Use_Data_Collector_If_Not_Specified_In_RunSettings_And_Not_Specified_In_ProjectFile_Async(UseDataCollectorElement useDataCollector)
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            mockCoverageProject.Setup(cp => cp.ProjectFileXElement).Returns(GetProjectElementWithDataCollector(useDataCollector));
            SetupDataCollectorState(mockCoverageProject, CoverletDataCollectorState.NotPresent);
            
            Assert.False(await coverletDataCollectorUtil.CanUseDataCollectorAsync(mockCoverageProject.Object));
        }

        [TestCase(UseDataCollectorElement.False)]
        [TestCase(UseDataCollectorElement.None)]
        public async Task Should_Not_Use_Data_Collector_If_No_RunSettings_And_Not_Specified_In_ProjectFile_Async(UseDataCollectorElement useDataCollector)
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            mockCoverageProject.Setup(cp => cp.RunSettingsFile).Returns((string)null);
            mockCoverageProject.Setup(cp => cp.ProjectFileXElement).Returns(GetProjectElementWithDataCollector(useDataCollector));

            Assert.False(await coverletDataCollectorUtil.CanUseDataCollectorAsync(mockCoverageProject.Object));
        }

        [TestCase(UseDataCollectorElement.True)]
        [TestCase(UseDataCollectorElement.False)]
        [TestCase(UseDataCollectorElement.None)]
        [TestCase(UseDataCollectorElement.Empty)]
        public async Task Should_Not_Use_Data_Collector_If_Disabled_In_RunSettings_Async(UseDataCollectorElement useDataCollector)
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            mockCoverageProject.Setup(cp => cp.ProjectFileXElement).Returns(GetProjectElementWithDataCollector(useDataCollector));
            SetupDataCollectorState(mockCoverageProject, CoverletDataCollectorState.Disabled);

            Assert.False(await coverletDataCollectorUtil.CanUseDataCollectorAsync(mockCoverageProject.Object));
        }

    }
}