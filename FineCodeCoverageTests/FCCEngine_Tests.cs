using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMoq;
using FineCodeCoverage.Collection.CoverageProjectManagement;
using FineCodeCoverage.Collection.CoverageToolOutput;
using FineCodeCoverage.Collection.CoverletOpenCover;
using FineCodeCoverage.Collection.Messages;
using FineCodeCoverage.Collection.ReportGeneration;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Engine.ReportGenerator;
using FineCodeCoverage.Options;
using FineCodeCoverage.Output;
using FineCodeCoverage.Utilities.Events;
using FineCodeCoverage.Utilities.Logging;
using Moq;
using NUnit.Framework;

namespace Test
{
    public class FCCEngine_ReloadCoverage_Tests
    {
        private AutoMoqer mocker;
        private FCCEngine fccEngine;
        private Mock<ICancellationTokenSource> mockCancellationTokenSource;
        private const string reportOutputFolder = "reportOutputFolder";
        private CancellationToken cancellationToken;

        [SetUp]
        public void SetUp()
        {
            Setup(false);
        }

        private void Setup(bool cancelled)
        {
            mocker = new AutoMoqer();
            mocker.Setup<ICoverageToolOutputManager, string>(
                coverageToolOutputManager => coverageToolOutputManager.GetReportOutputFolder()).Returns(reportOutputFolder);
            var mockDisposeAwareTaskRunner = mocker.GetMock<IDisposeAwareTaskRunner>();
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
            mockDisposeAwareTaskRunner.Setup(
                runner => runner.RunAsyncFunc(
                    It.IsAny<Func<Task>>())
                ).Callback<Func<Task>>(taskProvider => taskProvider().Wait());
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
            mockCancellationTokenSource = new Mock<ICancellationTokenSource>();
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;
            if (cancelled)
            {
                cancellationTokenSource.Cancel();
            }
            mockCancellationTokenSource.SetupGet(cts => cts.Token).Returns(cancellationToken);
            mockDisposeAwareTaskRunner.Setup(disposeAwareTaskRunner => disposeAwareTaskRunner.CreateLinkedTokenSource()).Returns(mockCancellationTokenSource.Object);
            fccEngine = mocker.Create<FCCEngine>();

        }

        [Test]
        public void Should_Prepare_For_Coverage_Suitable_CoverageProjects()
        {
            var mockSuitableCoverageProject = ReloadSuitableCoverageProject();
            mockSuitableCoverageProject.Verify(p => p.PrepareForCoverageAsync(cancellationToken,true));
        }

        [Test]
        public void Should_Set_Failure_Description_For_Unsuitable_Projects()
        {
            SetUpSuccessfulRunReportGenerator();

            var mockNullProjectFileProject = new Mock<ICoverageProject>();
            mockNullProjectFileProject.Setup(p => p.TestDllFile).Returns("Null_Project_File.dll");
            var mockWhitespaceProjectFileProject = new Mock<ICoverageProject>();
            mockWhitespaceProjectFileProject.Setup(p => p.ProjectFilePath).Returns("  ");
            mockWhitespaceProjectFileProject.Setup(p => p.TestDllFile).Returns("Whitespace_Project_File.dll");
            var mockDisabledProject = new Mock<ICoverageProject>();
            mockDisabledProject.Setup(p => p.ProjectFilePath).Returns("proj.csproj");
            mockDisabledProject.Setup(p => p.Settings.Enabled).Returns(false);
            
            ReloadInitializedCoverage(mockNullProjectFileProject.Object, mockWhitespaceProjectFileProject.Object, mockDisabledProject.Object);
            
            mockDisabledProject.VerifySet(p => p.FailureDescription = "Disabled");
            mockWhitespaceProjectFileProject.VerifySet(p => p.FailureDescription = "Unsupported project type for DLL 'Whitespace_Project_File.dll'");
            mockNullProjectFileProject.VerifySet(p => p.FailureDescription = "Unsupported project type for DLL 'Null_Project_File.dll'");
            
        }

        [Test]
        public void Should_Run_The_CoverTool_Step()
        {
            var mockCoverageProject = ReloadSuitableCoverageProject();
            mockCoverageProject.Verify(p => p.StepAsync("Run Coverage Tool", It.IsAny<Func<ICoverageProject, Task>>()));
        }

        [Test]
        public void Should_Run_Coverage_ThrowingErrors_But_Safely_With_Step()
        {
            ICoverageProject coverageProject = null;
            ReloadSuitableCoverageProject(mockCoverageProject => {
                coverageProject = mockCoverageProject.Object;
                mockCoverageProject.Setup(p => p.StepAsync("Run Coverage Tool", It.IsAny<Func<ICoverageProject, Task>>()))
                .Callback<string,Func<ICoverageProject, Task>>((_,runCoverTool) =>
                {
#pragma warning disable VSTHRD110 // Observe result of async calls
                    runCoverTool(coverageProject);
#pragma warning restore VSTHRD110 // Observe result of async calls
                });
            });

#pragma warning disable VSTHRD110 // Observe result of async calls
            mocker.Verify<ICoverageUtilManager>(coverageUtilManager => coverageUtilManager.RunCoverageAsync(coverageProject, cancellationToken));
#pragma warning restore VSTHRD110 // Observe result of async calls

        }

        [Test]
        public void Should_Allow_The_CoverageOutputManager_To_SetProjectCoverageOutputFolder()
        {
            var mockCoverageToolOutputManager = mocker.GetMock<ICoverageToolOutputManager>();
            mockCoverageToolOutputManager.Setup(om => om.SetProjectCoverageOutputFolderAsync(It.IsAny<List<ICoverageProject>>())).
                Callback<List<ICoverageProject>>(coverageProjects =>
                {
                    coverageProjects[0].CoverageOutputFolder = "Set by ICoverageToolOutputManager";
                });

            ICoverageProject coverageProjectAfterCoverageOutputManager = null;
            var coverageUtilManager = mocker.GetMock<ICoverageUtilManager>();
            coverageUtilManager.Setup(mgr => mgr.RunCoverageAsync(It.IsAny<ICoverageProject>(), cancellationToken))
                .Callback<ICoverageProject, CancellationToken>((cp, _) =>
                 {
                     coverageProjectAfterCoverageOutputManager = cp;
                 });

            ReloadSuitableCoverageProject(mockCoverageProject => {
                mockCoverageProject.SetupProperty(cp => cp.CoverageOutputFolder);
                mockCoverageProject.Setup(p => p.StepAsync("Run Coverage Tool", It.IsAny<Func<ICoverageProject, Task>>())).Callback<string, Func<ICoverageProject, Task>>((_, runCoverTool) =>
                {
#pragma warning disable VSTHRD110 // Observe result of async calls
                    runCoverTool(mockCoverageProject.Object);
#pragma warning restore VSTHRD110 // Observe result of async calls
                });
            });

            Assert.AreEqual(coverageProjectAfterCoverageOutputManager.CoverageOutputFolder, "Set by ICoverageToolOutputManager");
        }

        [Test]
        public void Should_Run_Report_Generator_With_Output_Files_From_Coverage_For_Coverage_Projects_That_Have_Not_Failed()
        {
            var failedProject = CreateSuitableProject();
            failedProject.Setup(p => p.HasFailed).Returns(true);
            
            var passedProjectCoverageOutputFile = "outputfile.xml";
            var passedProject = CreateSuitableProject();
            passedProject.Setup(p => p.CoverageOutputFile).Returns(passedProjectCoverageOutputFile);
            
            mocker.GetMock<IReportGeneratorUtil>().Setup(rg => 
                rg.GenerateAsync(
                    It.Is<string[]>(
                        coverOutputFiles => coverOutputFiles.Count() == 1 && coverOutputFiles.First() == passedProjectCoverageOutputFile),
                    reportOutputFolder,
                    cancellationToken
                    )).ReturnsAsync(new ReportGeneratorResult { });

            ReloadInitializedCoverage(failedProject.Object, passedProject.Object);

            mocker.GetMock<IReportGeneratorUtil>().VerifyAll();
            
        }

        [Test]
        public void Should_Not_Run_ReportGenerator_If_No_Successful_Projects()
        {
            ReloadInitializedCoverage();
#pragma warning disable VSTHRD110 // Observe result of async calls
            mocker.Verify<IReportGeneratorUtil>(rg => rg.GenerateAsync(
                It.IsAny<IEnumerable<string>>(), 
                It.IsAny<string>(), 
                It.IsAny<CancellationToken>()), Times.Never());
#pragma warning restore VSTHRD110 // Observe result of async calls
        }

        #region success path

        class SuccessState {
            public SuccessState(
                ICoverageProject coverageProject, 
                IReportResult reportResult,
                string coberturaFile)
            {
                CoverageProject = coverageProject;
                ReportResult = reportResult;
                CoberturaFile = coberturaFile;
            }

            public ICoverageProject CoverageProject { get; }
            public IReportResult ReportResult { get; }
            public string CoberturaFile { get; }
        }

        private SuccessState Run_Success()
        {
            var passedProject = CreateSuitableProject();
            var reportResult = new Mock<IReportResult>().Object;
            
            var reportGeneratorResult = new ReportGeneratorResult
            {
                UnifiedXmlFile = "unified.cobertura",
                ReportResult = reportResult
                
            };
            var mockReportGenerator = mocker.GetMock<IReportGeneratorUtil>();
            mockReportGenerator.Setup(rg =>
                rg.GenerateAsync(
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<string>(),
                    cancellationToken
                )).ReturnsAsync(reportGeneratorResult);

            ReloadInitializedCoverage(passedProject.Object);
            return new SuccessState(passedProject.Object,reportResult, "unified.cobertura");
        }

        [Test]
        public void Should_Log_Done_When_Have_The_ReportResult()
        {
            Run_Success();

            VerifyLogsCoverageStatus("Done");
        }

        [Test]
        public void Should_Send_CoverageEndedMessage_With_The_CoverageProjects_When_Have_The_ReportResult()
        {
            var successState = Run_Success();

            var coverageProject = successState.CoverageProject;
            mocker.Verify<IEventAggregator>(ea => ea.SendMessage(It.IsAny<CoverageEndedMessage>(), null));
        }

        [Test]
        public void Should_Send_NewReportMessage_When_Have_The_ReportResult()
        {
            var successState = Run_Success();

            var reportResult = successState.ReportResult;
            var coverageProject = successState.CoverageProject;
            mocker.Verify<IEventAggregator>(ea => ea.SendMessage(
                It.Is<NewReportMessage>(
                    msg => msg.Report == reportResult && 
                        msg.CoverageProjects.Count == 1 && 
                        msg.CoverageProjects[0] == coverageProject), null));
        }

        [Test]
        public void Should_Send_ReportFilesMessage_When_Have_The_ReportResult()
        {
            var successState = Run_Success();

            var reportResult = successState.ReportResult;
            var coberturaFile = successState.CoberturaFile;
            mocker.Verify<IEventAggregator>(
                ea => ea.SendMessage(
                    It.Is<ReportFilesMessage>(msg => msg.ReportResult == reportResult && msg.CoberturaFile == coberturaFile), null));
        }

        #endregion

        #region error path
        public Exception ThrowError()
        {
            var exception = new Exception("Error Message");
            fccEngine.ReloadCoverage(() => throw exception);
            return exception;
        }

        [Test]
        public void Should_Log_Error_When_Exception()
        {
            var exception = ThrowError();

#pragma warning disable VSTHRD110 // Observe result of async calls
            mocker.Verify<ILogger>(l => l.LogAsync(StatusMarkerProvider.Get("Error"), exception.ToString()));
#pragma warning restore VSTHRD110 // Observe result of async calls
        }

        [Test]
        public void Should_Raise_Coverage_Ended_Message_When_Exception()
        {
            ThrowError();
            mocker.Verify<IEventAggregator>(ea => ea.SendMessage(It.IsAny<CoverageEndedMessage>(), null));
        }

        #endregion

        [Test]
        public void Should_Cancel_Running_Coverage_Logging_Cancelled_When_StopCoverage()
        {
            ReloadInitializedCoverage();
            fccEngine.StopCoverage();
            mockCancellationTokenSource.Verify(cts => cts.Cancel());
        }

        [Test]
        public void Should_Not_Throw_When_StopCoverage_And_There_Is_No_Coverage_Running()
        {
            fccEngine.StopCoverage();
        } 

        [Test]
        public void Should_Cancel_Existing_ReloadCoverage_When_ReloadCoverage()
        {
            ReloadInitializedCoverage();
            mockCancellationTokenSource.Verify(cts => cts.Cancel(), Times.Never());
            ReloadInitializedCoverage();
            mockCancellationTokenSource.Verify(cts => cts.Cancel(), Times.Once());
        }

        [Test]
        public void Should_Log_Cancelled_When_Cancelled()
        {
            Setup(true);
            ReloadInitializedCoverage();
            VerifyLogsCoverageStatus("Cancelled");
        }

        private void VerifyLogsCoverageStatus(string coverageStatus)
        {
#pragma warning disable VSTHRD110 // Observe result of async calls
            mocker.Verify<ILogger>(l => l.LogAsync(StatusMarkerProvider.Get(coverageStatus)));
#pragma warning restore VSTHRD110 // Observe result of async calls
        }

        private void SetUpSuccessfulRunReportGenerator()
        {
            mocker.GetMock<IReportGeneratorUtil>()
                .Setup(rg => rg.GenerateAsync(
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                    ))
                .ReturnsAsync(new ReportGeneratorResult {  });
        }

        private void ReloadInitializedCoverage(params ICoverageProject[] coverageProjects)
        {
            var projectsFromTask = Task.FromResult(coverageProjects.ToList());
#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
            fccEngine.ReloadCoverage(() => projectsFromTask);
#pragma warning restore VSTHRD003 // Avoid awaiting foreign Tasks
        }

        private Mock<ICoverageProject> CreateSuitableProject()
        {
            var mockSuitableCoverageProject = new Mock<ICoverageProject>();
            mockSuitableCoverageProject.Setup(p => p.ProjectFilePath).Returns("Defined.csproj");
            mockSuitableCoverageProject.Setup(p => p.Settings.Enabled).Returns(true);
            mockSuitableCoverageProject.Setup(p => p.PrepareForCoverageAsync(It.IsAny<CancellationToken>(), true)).Returns(Task.FromResult(new CoverageProjectFileSynchronizationDetails()));
            mockSuitableCoverageProject.Setup(p => p.StepAsync("Run Coverage Tool", It.IsAny<Func<ICoverageProject, Task>>())).Returns(Task.CompletedTask);
            return mockSuitableCoverageProject;
        }

        private Mock<ICoverageProject> ReloadSuitableCoverageProject(
            Action<Mock<ICoverageProject>> setUp = null)
        {
            var mockSuitableCoverageProject = CreateSuitableProject();
            setUp?.Invoke(mockSuitableCoverageProject);
            SetUpSuccessfulRunReportGenerator();
            ReloadInitializedCoverage(mockSuitableCoverageProject.Object);
            return mockSuitableCoverageProject;
        }

    }

}