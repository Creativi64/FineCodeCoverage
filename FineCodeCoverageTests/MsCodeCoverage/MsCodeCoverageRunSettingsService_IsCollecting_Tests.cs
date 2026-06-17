using Moq;
using NUnit.Framework;
using AutoMoq;
using FineCodeCoverage.Collection.Ms;
using FineCodeCoverage.Collection.TestExplorer;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.IO;
using System;
using FineCodeCoverage.Collection.CoverageProjectManagement;
using FineCodeCoverage.Collection.CoverageProjectManagement.ReferencedProjects;
using FineCodeCoverage.Collection.CoverageToolOutput;
using FineCodeCoverage.Collection.CoverageProjectManagement.Settings;
using FineCodeCoverage.Initialization.ToolZip;
using FineCodeCoverage.Options.Base;
using FineCodeCoverage.Options.Run;
using FineCodeCoverage.Collection.Engine;
using FineCodeCoverage.VSAbstractions.OutputWindow;

namespace FineCodeCoverageTests.MsCodeCoverage
{
    internal class MsCodeCoverageRunSettingsService_StopCoverage_Test
    {
        [Test]
        public void Should_StopCoverage_On_FCCEngine()
        {
            var autoMocker = new AutoMoqer();

            var mockToolUnzipper = autoMocker.GetMock<IToolUnzipper>();
            mockToolUnzipper.Setup(tf => tf.EnsureUnzipped(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns("ZipDestination");

            var msCodeCoverageRunSettingsService = autoMocker.Create<MsCodeCoverageRunSettingsService>();

            msCodeCoverageRunSettingsService.StopCoverage();

            autoMocker.Verify<IFCCEngine>(fccEngine => fccEngine.StopCoverage());
        }
    }

    internal class UserRunSettingsAnalysisResult : IUserRunSettingsAnalysisResult
    {
        public UserRunSettingsAnalysisResult(bool suitable, bool specifiedMsCodeCoverage)
        {
            Suitable = suitable;
            SpecifiedMsCodeCoverage = specifiedMsCodeCoverage;
        }
        public UserRunSettingsAnalysisResult() { }

        public bool Suitable { get; set; }

        public bool SpecifiedMsCodeCoverage { get; set; }

        public List<ICoverageProject> ProjectsWithFCCMsTestAdapter { get; set; } = new List<ICoverageProject>();
    }

    internal class MsCodeCoverageRunSettingsService_IsCollecting_Tests
    {
        private AutoMoqer autoMocker;
        private MsCodeCoverageRunSettingsService msCodeCoverageRunSettingsService;
        private const string solutionDirectory = "SolutionDirectory";

        [SetUp]
        public void SetupSut()
        {
            autoMocker = new AutoMoqer();
            msCodeCoverageRunSettingsService = autoMocker.Create<MsCodeCoverageRunSettingsService>();
            SetupRunOptionsProvider();
        }

        [Test]
        public async Task Should_Try_Analyse_Projects_With_Runsettings_Async()
        {
            var fccMsTestAdapterPath = await InitializeFCCMsTestAdapterPathAsync();

            var coverageProjectWithRunSettings = CreateCoverageProject(".runsettings");
            var templatedCoverageProject = CreateCoverageProject(null);
            var coverageProjects = new List<ICoverageProject> { coverageProjectWithRunSettings, templatedCoverageProject };
            var testOperation = SetUpTestOperation(coverageProjects);

            await msCodeCoverageRunSettingsService.IsCollectingAsync(testOperation);

            // ms code coverage is always-on, so useMsCodeCoverage is always true
            autoMocker.Verify<IUserRunSettingsService>(
                userRunSettingsService => userRunSettingsService.Analyse(
                    new List<ICoverageProject> { coverageProjectWithRunSettings},
                    true,
                    fccMsTestAdapterPath)
                );

        }

        [Test] // in case shutdown visual studio before normal clean up operation
        public async Task Should_CleanUp_Projects_With_RunSettings_First_Async()
        {
            var coverageProjectWithRunSettings = CreateCoverageProject(".runsettings");
            var coverageProjects = new List<ICoverageProject> { coverageProjectWithRunSettings, CreateCoverageProject(null) };
            var testOperation = SetUpTestOperation(coverageProjects);

            var cleanedUp = false;
            var mockUserRunSettingsService = autoMocker.GetMock<IUserRunSettingsService>();
            mockUserRunSettingsService.Setup(
                userRunSettingsService => userRunSettingsService.Analyse(
                    It.IsAny<IEnumerable<ICoverageProject>>(),
                    It.IsAny<bool>(),
                    It.IsAny<string>()
                )
            ).Callback(() =>
            {
                Assert.True(cleanedUp);
            });

            var mockTemplatedRunSettingsService = autoMocker.GetMock<ITemplatedRunSettingsService>();
            mockTemplatedRunSettingsService.Setup(
                templatedRunSettingsService => 
                templatedRunSettingsService.CleanUpAsync(new List<ICoverageProject> { coverageProjectWithRunSettings})
            ).Callback(() =>
            {
                cleanedUp = true;
            });
            await msCodeCoverageRunSettingsService.IsCollectingAsync(testOperation);

            mockUserRunSettingsService.VerifyAll();
        }

        [Test]
        public async Task Should_Log_Exception_From_UserRunSettingsService_Analyse_Async()
        {
            var exception = new Exception("Msg");
            await Throw_Exception_From_UserRunSettingsService_Analyse_Async(exception);
            VerifyLogException("Exception analysing runsettings files", exception);
        }

        [Test]
        public async Task Should_Have_Status_Error_When_Exception_From_UserRunSettingsService_Analyse_Async()
        {
            var exception = new Exception("Msg");
            var status = await Throw_Exception_From_UserRunSettingsService_Analyse_Async(exception);
            Assert.AreEqual(MsCodeCoverageCollectionStatus.Error, status);
        }

        private Task<MsCodeCoverageCollectionStatus> Throw_Exception_From_UserRunSettingsService_Analyse_Async(Exception exception)
        {
            SetupIUserRunSettingsServiceAnalyseAny().Throws(exception);
            return msCodeCoverageRunSettingsService.IsCollectingAsync(SetUpTestOperation());
        }

        [Test]
        public async Task Should_Prepare_Coverage_Projects_When_Suitable_Async()
        {
            var mockTemplatedCoverageProject = new Mock<ICoverageProject>();
            var mockCoverageProjects = new List<Mock<ICoverageProject>>
            {
                mockTemplatedCoverageProject,
                CreateMinimalMockRunSettingsCoverageProject()
        };
            var coverageProjects = mockCoverageProjects.Select(mockCoverageProject => mockCoverageProject.Object).ToList();
            var testOperation = SetUpTestOperation(coverageProjects);

            SetupIUserRunSettingsServiceAnalyseAny().Returns(new UserRunSettingsAnalysisResult(true, false));

            await msCodeCoverageRunSettingsService.IsCollectingAsync(testOperation);

#pragma warning disable VSTHRD110 // Observe result of async calls
            autoMocker.Verify<ICoverageToolOutputManager>(coverageToolOutputManager => coverageToolOutputManager.SetProjectCoverageOutputFolderAsync(coverageProjects));
#pragma warning restore VSTHRD110 // Observe result of async calls
            foreach (var mockCoverageProject in mockCoverageProjects)
            {
                mockCoverageProject.Verify(coverageProject => coverageProject.PrepareForCoverageAsync(CancellationToken.None, false));
            }
        }

        [Test]
        public async Task Should_Set_UserRunSettingsProjectDetailsLookup_For_IRunSettingsService_When_Suitable_Async()
        {
            var projectSettings = new Mock<ICoverageSettings>().Object;
            var excludedReferencedProjects = new List<IReferencedProject>();
            var includedReferencedProjects = new List<IReferencedProject>();
            var coverageProjects = new List<ICoverageProject>
            {
                CreateCoverageProject(null),
                CreateCoverageProject(
                    ".runsettings",
                    projectSettings,
                    "OutputFolder",
                    "Test.dll",
                    excludedReferencedProjects, 
                    includedReferencedProjects
                )
            };
            var testOperation = SetUpTestOperation(coverageProjects);

            SetupIUserRunSettingsServiceAnalyseAny().Returns(new UserRunSettingsAnalysisResult(true, false));

            await msCodeCoverageRunSettingsService.IsCollectingAsync(testOperation);

            // The templated project has no test dll (CreateCoverageProject(null)) so only the project with a
            // test dll is in the lookup; both are nonetheless injected via AddFCCRunSettings at run time.
            var userRunSettingsProjectDetailsLookup = msCodeCoverageRunSettingsService.UserRunSettingsProjectDetailsLookup;
            Assert.AreEqual(1, userRunSettingsProjectDetailsLookup.Count);
            var userRunSettingsProjectDetails = userRunSettingsProjectDetailsLookup["Test.dll"];
            Assert.AreSame(projectSettings, userRunSettingsProjectDetails.Settings);
            Assert.AreSame(excludedReferencedProjects, userRunSettingsProjectDetails.ExcludedReferencedProjects);
            Assert.AreSame(includedReferencedProjects, userRunSettingsProjectDetails.IncludedReferencedProjects);
            Assert.AreEqual("OutputFolder", userRunSettingsProjectDetails.CoverageOutputFolder);
            Assert.AreEqual("Test.dll", userRunSettingsProjectDetails.TestDllFile);
        }

        [Test]
        public async Task Should_Add_Templated_Projects_To_The_Lookup_For_In_Memory_Injection_Async()
        {
            // Core of the in-memory-injection fix: a project WITHOUT its own runsettings (templated) is now
            // injected via AddFCCRunSettings just like a user-runsettings project, so both - keyed by test
            // dll - must be in the lookup (no project files are mutated).
            var coverageProjects = new List<ICoverageProject>
            {
                CreateCoverageProject(
                    null, new Mock<ICoverageSettings>().Object, "TemplatedOutput", "Templated.dll",
                    new List<IReferencedProject>(), new List<IReferencedProject>()),
                CreateCoverageProject(
                    ".runsettings", new Mock<ICoverageSettings>().Object, "RunSettingsOutput", "WithRunSettings.dll",
                    new List<IReferencedProject>(), new List<IReferencedProject>())
            };
            var testOperation = SetUpTestOperation(coverageProjects);
            SetupIUserRunSettingsServiceAnalyseAny().Returns(new UserRunSettingsAnalysisResult(true, false));

            await msCodeCoverageRunSettingsService.IsCollectingAsync(testOperation);

            var lookup = msCodeCoverageRunSettingsService.UserRunSettingsProjectDetailsLookup;
            Assert.AreEqual(2, lookup.Count);
            Assert.AreEqual("TemplatedOutput", lookup["Templated.dll"].CoverageOutputFolder);
            Assert.AreEqual("RunSettingsOutput", lookup["WithRunSettings.dll"].CoverageOutputFolder);
        }

        [Test]
        public async Task Should_Be_Collecting_When_Suitable_RunSettings_And_No_Templates_Async()
        {
            var status = await IsCollecting_With_Suitable_RunSettings_Only_Async();
            Assert.AreEqual(MsCodeCoverageCollectionStatus.Collecting, status);
        }

        [Test]
        public async Task Should_Log_Ms_Code_Coverage_When_Collecting_Async()
        {
            await IsCollecting_With_Suitable_RunSettings_Only_Async();
#pragma warning disable VSTHRD110 // Observe result of async calls
            autoMocker.Verify<ILogger>(l => l.LogAsync("Ms code coverage"));
#pragma warning restore VSTHRD110 // Observe result of async calls
        }

        private Task<MsCodeCoverageCollectionStatus> IsCollecting_With_Suitable_RunSettings_Only_Async()
        {
            var testOperation = SetUpTestOperation();
            SetupIUserRunSettingsServiceAnalyseAny().Returns(new UserRunSettingsAnalysisResult(true, false));
            return msCodeCoverageRunSettingsService.IsCollectingAsync(testOperation);
        }

        [Test]
        public async Task Should_Not_Be_Collecting_If_User_RunSettings_Are_Not_Suitable_Async()
        {
            var testOperation = SetUpTestOperation();
            SetupIUserRunSettingsServiceAnalyseAny().Returns(new UserRunSettingsAnalysisResult());

            var collectionStatus = await msCodeCoverageRunSettingsService.IsCollectingAsync(testOperation);
            Assert.AreEqual(MsCodeCoverageCollectionStatus.NotCollecting, collectionStatus);
        }

        [Test]
        public async Task Should_Shim_Copy_For_All_Collected_Projects_Async()
        {
            // Coverage is injected in-memory for every collected project (via AddFCCRunSettings), so the shim
            // - needed wherever the FCC ms test adapter is used - is copied for all of them.  The shim copier
            // itself filters to .NET Framework projects (covered by ShimCopier_Tests).
            var shimPath = await InitializeShimPathAsync();
            SetupIUserRunSettingsServiceAnalyseAny().Returns(new UserRunSettingsAnalysisResult(true, false));

            var coverageProjects = new List<ICoverageProject>
            {
                CreateCoverageProject(".runsettings"),
                CreateCoverageProject(null)
            };
            var testOperation = SetUpTestOperation(coverageProjects);

            await msCodeCoverageRunSettingsService.IsCollectingAsync(testOperation);

            autoMocker.Verify<IShimCopier>(shimCopier => shimCopier.Copy(shimPath, coverageProjects));
        }

        private Moq.Language.Flow.ISetup<IUserRunSettingsService, IUserRunSettingsAnalysisResult> SetupIUserRunSettingsServiceAnalyseAny()
        {
            var mockUserRunSettingsService = autoMocker.GetMock<IUserRunSettingsService>();
            return mockUserRunSettingsService.Setup(userRunSettingsService => userRunSettingsService.Analyse(
                It.IsAny<List<ICoverageProject>>(),
                It.IsAny<bool>(),
                It.IsAny<string>()
            ));
        }

        private ITestOperation SetUpTestOperation(List<ICoverageProject> coverageProjects = null)
        {
            coverageProjects = coverageProjects ?? new List<ICoverageProject>();
            var mockTestOperation = new Mock<ITestOperation>();
            mockTestOperation.Setup(testOperation => testOperation.GetCoverageProjectsAsync()).ReturnsAsync(coverageProjects);
            mockTestOperation.Setup(testOperation => testOperation.SolutionDirectory).Returns(solutionDirectory);
            return mockTestOperation.Object;
        }

        private void SetupRunOptionsProvider()
        {
            var mockRunOptionsProvider = autoMocker.GetMock<IOptionsProvider<RunOptions>>();
            mockRunOptionsProvider.Setup(p => p.Get())
                .Returns(new RunOptions());
        }

        private void VerifyLogException(string reason, Exception exception)
        {
#pragma warning disable VSTHRD110 // Observe result of async calls
            autoMocker.Verify<ILogger>(l => l.LogAsync(reason, exception.ToString()));
#pragma warning restore VSTHRD110 // Observe result of async calls
        }

        private async Task<string> InitializeFCCMsTestAdapterPathAsync()
        {
            await InitializeZipDestinationAsync();
            return Path.Combine("ZipDestination", "build", "netstandard2.0");
        }

        private async Task<string> InitializeShimPathAsync()
        {
            await InitializeZipDestinationAsync();
            return Path.Combine("ZipDestination", "build", "netstandard2.0", "CodeCoverage", "coreclr", "Microsoft.VisualStudio.CodeCoverage.Shim.dll");
        }

        private Task InitializeZipDestinationAsync()
        {
            var mockToolUnzipper = autoMocker.GetMock<IToolUnzipper>();
            mockToolUnzipper.Setup(tf => tf.EnsureUnzipped(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns("ZipDestination");
            return msCodeCoverageRunSettingsService.InitializeAsync(null, CancellationToken.None);
        }

        private ICoverageProject CreateCoverageProject(
            string runSettingsFile,
            ICoverageSettings settings = null,
            string coverageOutputFolder = "",
            string testDllFile = "", 
            List<IReferencedProject> excludedReferencedProjects = null,
            List<IReferencedProject> includedReferencedProjects = null,
            string projectFile = ""
        )
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            mockCoverageProject.Setup(cp => cp.RunSettingsFile).Returns(runSettingsFile);
            mockCoverageProject.Setup(cp => cp.CoverageOutputFolder).Returns(coverageOutputFolder);
            mockCoverageProject.Setup(cp => cp.TestDllFile).Returns(testDllFile);
            mockCoverageProject.Setup(cp => cp.ExcludedReferencedProjects).Returns(excludedReferencedProjects);
            mockCoverageProject.Setup(cp => cp.IncludedReferencedProjects).Returns(includedReferencedProjects);
            mockCoverageProject.Setup(cp => cp.Settings).Returns(settings);
            mockCoverageProject.Setup(cp => cp.ProjectFilePath).Returns(projectFile);
            return mockCoverageProject.Object;
        }

        private Mock<ICoverageProject> CreateMinimalMockRunSettingsCoverageProject()
        {
            var mockCoverageProjectWithRunSettings = new Mock<ICoverageProject>();
            mockCoverageProjectWithRunSettings.Setup(cp => cp.RunSettingsFile).Returns(".runsettings");
            mockCoverageProjectWithRunSettings.Setup(cp => cp.TestDllFile).Returns("Test.dll");
            return mockCoverageProjectWithRunSettings;
        }
    }
}
