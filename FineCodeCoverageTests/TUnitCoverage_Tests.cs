using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMoq;
using FineCodeCoverage.Collection.CoverageProjectManagement;
using FineCodeCoverage.Collection.Engine;
using FineCodeCoverage.Collection.Runners;
using FineCodeCoverage.Collection.TestingPlatform.TUnit;
using FineCodeCoverage.Utilities.Wrappers;
using FineCodeCoverage.VSAbstractions.Threading;
using Moq;
using NUnit.Framework;

namespace FineCodeCoverageTests
{
    public class TUnitCoverage_Tests
    {
        private AutoMoqer mocker;
        private List<ITUnitProject> projects;

        [SetUp]
        public void SetUp()
        {
            mocker = new AutoMoqer();
            projects = new List<ITUnitProject>();
            mocker.GetMock<ITUnitProjectsProvider>()
                .Setup(provider => provider.GetTUnitProjectsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(projects);
            mocker.GetMock<IDisposeAwareTaskRunner>()
                .Setup(runner => runner.CreateLinkedTokenSource())
                .Returns(Mock.Of<ICancellationTokenSource>());
        }

        private ITUnitCoverageProject AddProject(string name, bool hasExtension, bool injected, bool hasOutput)
        {
            ITUnitProject project = Mock.Of<ITUnitProject>();
            projects.Add(project);
            var coverageProject = new Mock<ICoverageProject>();
            coverageProject.Setup(p => p.ProjectName).Returns(name);
            coverageProject.Setup(p => p.ProjectFilePath).Returns(name + ".csproj");
            coverageProject.Setup(p => p.Settings.Enabled).Returns(true);
            var mtpProject = new Mock<ITUnitCoverageProject>();
            mtpProject.Setup(p => p.CoverageProject).Returns(coverageProject.Object);
            mtpProject.Setup(p => p.HasCoverageExtension).Returns(hasExtension);
            mocker.GetMock<ITUnitCoverageProjectFactory>()
                .Setup(factory => factory.CreateTUnitCoverageProjectAsync(project, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mtpProject.Object);
            var settings = new TUnitSettings(name + ".exe", "settings.xml", false, name + ".xml", null, null, string.Empty);
            mocker.GetMock<ITUnitSettingsProvider>()
                .Setup(provider => provider.ProvideAsync(mtpProject.Object, It.IsAny<CancellationToken>()))
                .ReturnsAsync(settings);
            mocker.GetMock<ITestConfigInjector>()
                .Setup(injector => injector.InjectAsync(mtpProject.Object, settings, It.IsAny<CancellationToken>()))
                .ReturnsAsync(injected ? new TestConfigInjectionResult(name + ".testconfig.json", settings.OutputPath, "cobertura", "settings.xml") : null);
            mocker.GetMock<IFileUtil>().Setup(files => files.Exists(settings.OutputPath)).Returns(hasOutput);
            return mtpProject.Object;
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task Should_Never_ReRun_A_Test_Host_When_Injection_Is_Unavailable_Async(bool hasExtension)
        {
            AddProject("Unsupported", hasExtension, false, true);
            var collector = mocker.Create<TUnitCoverage>();

            IReadOnlyCollection<string> ownedProjects = await collector.PrepareForTestRunAsync();
            CoverageCollectionResult result = await collector.CollectAfterTestRunAsync();

            Assert.That(ownedProjects, Is.EquivalentTo(new[] { "Unsupported.csproj" }));
            Assert.That(result.HasCoverage, Is.False);
            AssertNoHostRuns();
        }

        [Test]
        public async Task Should_Keep_Coverage_From_The_Selected_Run_When_Other_Projects_Are_Skipped_Async()
        {
            AddProject("Selected", true, true, true);
            AddProject("NotSelected", true, true, false);
            AddProject("Unsupported", false, false, true);
            var collector = mocker.Create<TUnitCoverage>();

            await collector.PrepareForTestRunAsync();
            CoverageCollectionResult result = await collector.CollectAfterTestRunAsync();

            Assert.That(result.CoberturaFiles, Is.EqualTo(new[] { "Selected.xml" }));
            AssertNoHostRuns();
        }

        [Test]
        public async Task Should_Still_Run_Test_Hosts_When_Manually_Requested_Async()
        {
            ITUnitCoverageProject project = AddProject("Manual", false, false, true);
            mocker.GetMock<IBuildHelper>()
                .Setup(helper => helper.BuildAsync(It.IsAny<List<Microsoft.VisualStudio.Shell.Interop.IVsHierarchy>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            mocker.GetMock<ITUnitCoverageRunner>()
                .Setup(runner => runner.RunAsync(It.IsAny<TUnitSettings>(), false, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            Func<Task> collect = null;
            mocker.GetMock<IDisposeAwareTaskRunner>()
                .Setup(runner => runner.RunAsyncFunc(It.IsAny<Func<Task>>()))
                .Callback<Func<Task>>(action => collect = action);

            mocker.Create<TUnitCoverage>().CollectCoverage();
            await collect();

            mocker.GetMock<IFCCEngine>().Verify(engine => engine.RunAndProcessReport(
                It.Is<string[]>(files => files.Length == 1 && files[0] == "Manual.xml"),
                It.Is<List<ICoverageProject>>(items => items.Contains(project.CoverageProject)),
                null), Times.Once());
        }

        private void AssertNoHostRuns()
            => mocker.GetMock<ITUnitCoverageRunner>().Verify(runner => runner.RunAsync(
                It.IsAny<TUnitSettings>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never());
    }
}
