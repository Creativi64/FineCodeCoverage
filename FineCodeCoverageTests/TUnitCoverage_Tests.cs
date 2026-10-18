using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMoq;
using FineCodeCoverage.Collection.CoverageProjectManagement;
using FineCodeCoverage.Collection.Engine;
using FineCodeCoverage.Collection.Runners;
using FineCodeCoverage.Collection.TestingPlatform.TUnit;
using FineCodeCoverage.Utilities.Wrappers;
using FineCodeCoverage.VSAbstractions.Threading;
using FineCodeCoverage.Vs.Events;
using FineCodeCoverageTests.TestHelpers;
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
                It.Is<CoverageCollectionResult>(result => result.CoberturaFiles.Count == 1 && result.CoberturaFiles[0] == "Manual.xml"
                    && result.CoverageProjects.Contains(project.CoverageProject)),
                null), Times.Once());
        }

        private void AssertNoHostRuns()
            => mocker.GetMock<ITUnitCoverageRunner>().Verify(runner => runner.RunAsync(
                It.IsAny<TUnitSettings>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never());

        [Test]
        public async Task Should_Restore_A_Prepared_Config_When_Visual_Studio_Shuts_Down_Async()
        {
            AddProject("Selected", true, true, true);
            var collector = mocker.Create<TUnitCoverage>();
            await collector.PrepareForTestRunAsync();

            mocker.GetMock<IVsShutdown>().Raise(shutdown => shutdown.Shutdown += null, EventArgs.Empty);

            mocker.GetMock<ITestConfigInjector>().Verify(injector => injector.Restore(
                It.Is<TestConfigInjectionResult>(result => result.TestConfigPath == "Selected.testconfig.json")), Times.Once());
            Assert.That((await collector.CollectAfterTestRunAsync()).HasCoverage, Is.False);
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task Should_Restore_Injections_After_Completion_Or_Cancellation_Async(bool cancel)
        {
            AddProject("Selected", true, true, true);
            var collector = mocker.Create<TUnitCoverage>();
            await collector.PrepareForTestRunAsync();
            if (cancel)
                collector.Cancel();
            else
                await collector.CollectAfterTestRunAsync();

            mocker.GetMock<ITestConfigInjector>().Verify(injector => injector.Restore(
                It.Is<TestConfigInjectionResult>(result => result.TestConfigPath == "Selected.testconfig.json")), Times.Once());
        }

        [Test]
        public async Task Should_Restore_Previous_Injections_If_Preparation_Fails_Async()
        {
            AddProject("First", true, true, true);
            ITUnitCoverageProject second = AddProject("Second", true, true, true);
            mocker.GetMock<ITUnitSettingsProvider>().Setup(provider => provider.ProvideAsync(second, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("preparation failed"));

            IReadOnlyCollection<string> prepared = await mocker.Create<TUnitCoverage>().PrepareForTestRunAsync();

            Assert.That(prepared, Is.Empty);
            mocker.GetMock<ITestConfigInjector>().Verify(injector => injector.Restore(
                It.Is<TestConfigInjectionResult>(result => result.TestConfigPath == "First.testconfig.json")), Times.Once());
        }

        [Test]
        public async Task Should_Not_Dispose_The_New_Run_When_An_Older_Preparation_Finishes_Async()
        {
            AddProject("Selected", true, true, true);
            var oldSource = new TrackingCancellationSource();
            var newSource = new TrackingCancellationSource();
            mocker.GetMock<IDisposeAwareTaskRunner>().SetupSequence(runner => runner.CreateLinkedTokenSource())
                .Returns(oldSource).Returns(newSource);
            var delayedProjects = new TaskCompletionSource<List<ITUnitProject>>();
            mocker.GetMock<ITUnitProjectsProvider>().SetupSequence(provider => provider.GetTUnitProjectsAsync(It.IsAny<CancellationToken>()))
                .Returns(delayedProjects.Task).ReturnsAsync(projects);
            var collector = mocker.Create<TUnitCoverage>();

            Task<IReadOnlyCollection<string>> oldRun = collector.PrepareForTestRunAsync();
            Task<IReadOnlyCollection<string>> newRun = collector.PrepareForTestRunAsync();
            delayedProjects.SetResult(projects);
            await oldRun;
            await newRun;

            Assert.That(oldSource.Disposed, Is.True);
            Assert.That(newSource.Disposed, Is.False);
            Assert.That(newSource.IsCancellationRequested, Is.False);
            collector.Cancel();
            Assert.That(newSource.Disposed, Is.True);
        }

        [Test]
        public async Task Should_Report_Collected_And_Skipped_Projects_With_Reasons_Async()
        {
            AddProject("Selected", true, true, true);
            AddProject("MissingExtension", false, false, false);
            AddProject("NotSelected", true, true, false);
            var collector = mocker.Create<TUnitCoverage>();
            await collector.PrepareForTestRunAsync();
            CoverageCollectionResult result = await collector.CollectAfterTestRunAsync();

            Assert.That(result.Summary, Is.EqualTo("Coverage collected for 1 of 3 test projects."));
            Assert.That(result.Details, Does.Contain("MissingExtension: Microsoft.Testing.Extensions.CodeCoverage is missing"));
            Assert.That(result.Details, Does.Contain("NotSelected: No coverage file was produced"));
        }
    }
}
