using NUnit.Framework;
using FineCodeCoverage.Collection.Ms;
using AutoMoq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Moq;
using FineCodeCoverage.Collection.CoverageProjectManagement;

namespace FineCodeCoverageTests.MsCodeCoverage
{
    internal class TemplatedRunSettingsService_Tests
    {
        private AutoMoqer autoMocker;
        private TemplatedRunSettingsService templatedRunSettingsService;

        [SetUp]
        public void SetupSut()
        {
            autoMocker = new AutoMoqer();
            templatedRunSettingsService = autoMocker.Create<TemplatedRunSettingsService>();
        }

        [Test]
        public async Task Clean_Up_Should_Remove_Generated_Project_RunSettings_Async()
        {
            var coverageProjects = new List<ICoverageProject> { new Mock<ICoverageProject>().Object };
            await templatedRunSettingsService.CleanUpAsync(coverageProjects);
#pragma warning disable VSTHRD110 // Observe result of async calls
            autoMocker.Verify<IProjectRunSettingsGenerator>(
                projectRunSettingsGenerator => projectRunSettingsGenerator.RemoveGeneratedProjectSettingsAsync(coverageProjects)
            );
#pragma warning restore VSTHRD110 // Observe result of async calls
        }
    }
}
