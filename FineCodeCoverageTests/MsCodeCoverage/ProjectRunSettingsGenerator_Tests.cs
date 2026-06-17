using NUnit.Framework;
using FineCodeCoverage.Collection.Ms;
using AutoMoq;
using System.Threading.Tasks;
using Moq;
using System;
using FineCodeCoverage.Collection.CoverageProjectManagement;

namespace FineCodeCoverageTests.MsCodeCoverage
{
    internal class ProjectRunSettingsGenerator_Tests
    {
        private AutoMoqer autoMocker;
        private ProjectRunSettingsGenerator projectRunSettingsGenerator;

        [SetUp]
        public void Setup()
        {
            autoMocker = new AutoMoqer();
            projectRunSettingsGenerator = autoMocker.Create<ProjectRunSettingsGenerator>();
        }

        [Test]
        public async Task Should_Remove_Generated_Run_Settings_File_Path_With_The_VsRunSettingsWriter_Async()
        {
            var projectIdWithGenerated = Guid.NewGuid();

            ICoverageProject SetupProject(string runSettingsFilePath, Guid id)
            {
                var mockCoverageProject = new Mock<ICoverageProject>();
                mockCoverageProject.Setup(cp => cp.RunSettingsFile).Returns(runSettingsFilePath);
                mockCoverageProject.Setup(cp => cp.Id).Returns(id);
                return mockCoverageProject.Object;
            }

            var withGenerated = SetupProject("Project1-fcc-mscodecoverage-generated.runsettings", projectIdWithGenerated);
            var withoutRunSettings = SetupProject(null, Guid.NewGuid());
            var withNonGeneratedRunSettings = SetupProject("", Guid.NewGuid());

            await projectRunSettingsGenerator.RemoveGeneratedProjectSettingsAsync(
                new[] { withGenerated, withoutRunSettings, withNonGeneratedRunSettings });

            var mockVsRunSettingsWriter = autoMocker.GetMock<IVsRunSettingsWriter>();
            mockVsRunSettingsWriter.Verify(rsw => rsw.RemoveRunSettingsFilePathAsync(projectIdWithGenerated));
            mockVsRunSettingsWriter.VerifyNoOtherCalls();
        }
    }
}
