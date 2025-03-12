using Moq;
using NUnit.Framework;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using System;
using System.Collections.Generic;
using AutoMoq;
using System.Threading;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Impl;
using FineCodeCoverage.Engine;
using System.Threading.Tasks;
using FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Options;
using ILogger = FineCodeCoverage.Output.ILogger;
using System.Linq;


namespace FineCodeCoverageTests.MsCodeCoverage
{

    internal class MsCodeCoverageRunSettingsService_Test_Execution_Not_Finished_Tests
    {
        [Test]
        public async Task Should_Set_To_Not_Collecting_Async()
        {
            var autoMocker = new AutoMoqer();
            var msCodeCoverageRunSettingsService = autoMocker.Create<MsCodeCoverageRunSettingsService>();

            msCodeCoverageRunSettingsService.collectionStatus = MsCodeCoverageCollectionStatus.Collecting;

            var mockTestOperation = new Mock<ITestOperation>();
            mockTestOperation.Setup(testOperation => testOperation.GetCoverageProjectsAsync()).ReturnsAsync(new List<ICoverageProject>());
            await msCodeCoverageRunSettingsService.TestExecutionNotFinishedAsync(mockTestOperation.Object);

            Assert.That(msCodeCoverageRunSettingsService.collectionStatus, Is.EqualTo(MsCodeCoverageCollectionStatus.NotCollecting));
        }

        [Test]
        public async Task Should_Clean_Up_RunSettings_Coverage_Projects_Async()
        {
            var autoMocker = new AutoMoqer();
            var msCodeCoverageRunSettingsService = autoMocker.Create<MsCodeCoverageRunSettingsService>();


            var mockUserRunSettingsService = autoMocker.GetMock<IUserRunSettingsService>();
            mockUserRunSettingsService.Setup(
                userRunSettingsService =>
                userRunSettingsService.Analyse(It.IsAny<IEnumerable<ICoverageProject>>(), It.IsAny<bool>(), It.IsAny<string>())
            ).Returns(new UserRunSettingsAnalysisResult());

            var mockAppOptionsProvider = autoMocker.GetMock<IAppOptionsProvider>();
            mockAppOptionsProvider.Setup(appOptionsProvider => appOptionsProvider.Get()).Returns(new Mock<IAppOptions>().Object);

            // is collecting
            var mockTestOperation = new Mock<ITestOperation>();
            var runSettingsCoverageProject = CreateCoverageProject(".runsettings");
            var coverageProjects = new List<ICoverageProject>
            {
                runSettingsCoverageProject,
                CreateCoverageProject(null)
                
            };
            mockTestOperation.Setup(testOperation => testOperation.GetCoverageProjectsAsync()).ReturnsAsync(coverageProjects);
            
            await msCodeCoverageRunSettingsService.IsCollectingAsync(mockTestOperation.Object);

            await msCodeCoverageRunSettingsService.TestExecutionNotFinishedAsync(mockTestOperation.Object);

#pragma warning disable VSTHRD110 // Observe result of async calls
            autoMocker.Verify<ITemplatedRunSettingsService>(
                templatedRunSettingsService => templatedRunSettingsService.CleanUpAsync(new List<ICoverageProject> { runSettingsCoverageProject })
            );
#pragma warning restore VSTHRD110 // Observe result of async calls
        }

        private ICoverageProject CreateCoverageProject(string runSettingsFile)
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            mockCoverageProject.Setup(coverageProject => coverageProject.RunSettingsFile).Returns(runSettingsFile);
            return mockCoverageProject.Object;
        }
    }

    internal class MsCodeCoverageRunSettingsService_Collect_Tests
    {
        private AutoMoqer autoMocker;
        private ICoverageProject runSettingsCoverageProject;
        private MsCodeCoverageRunSettingsService msCodeCoverageRunSettingsService;

        [Test]
        public async Task Should_Set_To_Not_Collecting_Async()
        {
            var resultsUris = new List<Uri>()
            {
                new Uri(@"C:\SomePath\result1.cobertura.xml", UriKind.Absolute),
                new Uri(@"C:\SomePath\result2.cobertura.xml", UriKind.Absolute),
                new Uri(@"C:\SomePath\result3.xml", UriKind.Absolute),
            };

            var expectedCoberturaFiles = new string[] { @"C:\SomePath\result1.cobertura.xml", @"C:\SomePath\result2.cobertura.xml" };
            await RunAndProcessReportAsync(resultsUris, expectedCoberturaFiles);

            Assert.That(msCodeCoverageRunSettingsService.collectionStatus, Is.EqualTo(MsCodeCoverageCollectionStatus.NotCollecting));
        }

        [Test]
        public async Task Should_FCCEngine_RunAndProcessReport_With_CoberturaResults_Async()
        {
            var resultsUris = new List<Uri>()
            {
                new Uri(@"C:\SomePath\result1.cobertura.xml", UriKind.Absolute),
                new Uri(@"C:\SomePath\result2.cobertura.xml", UriKind.Absolute),
                new Uri(@"C:\SomePath\result3.xml", UriKind.Absolute),
            };

            var expectedCoberturaFiles = new string[] { @"C:\SomePath\result1.cobertura.xml", @"C:\SomePath\result2.cobertura.xml" };
            await RunAndProcessReportAsync(resultsUris, expectedCoberturaFiles);
        }

        [Test]
        public async Task Should_Not_Throw_If_No_Results_Async()
        {
            await RunAndProcessReportAsync(null, Array.Empty<string>());
        }

        [Test]
        public async Task Should_Log_When_No_Cobertura_Files_Async()
        {
            await RunAndProcessReportAsync(null, Array.Empty<string>());
#pragma warning disable VSTHRD110 // Observe result of async calls
            autoMocker.Verify<ILogger>(logger => logger.LogAsync("No cobertura files for ms code coverage."));
#pragma warning restore VSTHRD110 // Observe result of async calls
        }

        [Test]
        public async Task Should_Clean_Up_RunSettings_Coverage_Projects_From_IsCollecting_Async()
        {
            await RunAndProcessReportAsync(null, Array.Empty<string>());
#pragma warning disable VSTHRD110 // Observe result of async calls
            autoMocker.Verify<ITemplatedRunSettingsService>(
                templatedRunSettingsService => templatedRunSettingsService.CleanUpAsync(new List<ICoverageProject> { runSettingsCoverageProject })
            );
#pragma warning restore VSTHRD110 // Observe result of async calls
        }

        private async Task RunAndProcessReportAsync(IEnumerable<Uri> resultsUris,string[] expectedCoberturaFiles)
        {
            autoMocker = new AutoMoqer();
            var mockToolUnzipper = autoMocker.GetMock<IToolUnzipper>();
            mockToolUnzipper.Setup(tf => tf.EnsureUnzipped(
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<CancellationToken>()
            )).Returns("ZipDestination");
            
            msCodeCoverageRunSettingsService = autoMocker.Create<MsCodeCoverageRunSettingsService>();
            msCodeCoverageRunSettingsService.collectionStatus = MsCodeCoverageCollectionStatus.Collecting;

            var mockFccEngine = new Mock<IFCCEngine>();
            msCodeCoverageRunSettingsService.Initialize("", mockFccEngine.Object, CancellationToken.None);

            var mockOperation = new Mock<IOperation>();
            mockOperation.Setup(operation => operation.GetRunSettingsDataCollectorResultUri(new Uri(RunSettingsHelper.MsDataCollectorUri))).Returns(resultsUris);
            

            // IsCollecting
            var mockTestOperation = new Mock<ITestOperation>();
            runSettingsCoverageProject = CreateCoverageProject(".runsettings");
            var coverageProjects = new List<ICoverageProject>
            {
                CreateCoverageProject(null),
                runSettingsCoverageProject
            };
            mockTestOperation.Setup(testOperation => testOperation.GetCoverageProjectsAsync()).ReturnsAsync(coverageProjects);
            var mockAppOptionsProvider = autoMocker.GetMock<IAppOptionsProvider>();
            mockAppOptionsProvider.Setup(appOptionsProvider => appOptionsProvider.Get()).Returns(new Mock<IAppOptions>().Object);
            await msCodeCoverageRunSettingsService.IsCollectingAsync(mockTestOperation.Object);

            await msCodeCoverageRunSettingsService.CollectAsync(mockOperation.Object, mockTestOperation.Object);


            mockFccEngine.Verify(engine => engine.RunAndProcessReport(
                    It.Is<string[]>(coberturaFiles => !expectedCoberturaFiles.Except(coberturaFiles).Any() && !coberturaFiles.Except(expectedCoberturaFiles).Any()), 
                    coverageProjects,null
                )
            );
        }

        private ICoverageProject CreateCoverageProject(string runSettingsFile)
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            mockCoverageProject.Setup(coverageProject => coverageProject.RunSettingsFile).Returns(runSettingsFile);
            return mockCoverageProject.Object;
        }
    }
}
