using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMoq;
using FineCodeCoverage.Collection.CoverageProjectManagement;
using FineCodeCoverage.Collection.Engine;
using FineCodeCoverage.Collection.Messages;
using FineCodeCoverage.Collection.Ms;
using FineCodeCoverage.Collection.Runners;
using FineCodeCoverage.Collection.TestExplorer;
using FineCodeCoverage.Initialization;
using FineCodeCoverage.Options.Base;
using FineCodeCoverage.Options.Run;
using FineCodeCoverage.Utilities.Events;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using Moq;
using NUnit.Framework;
using ILogger = FineCodeCoverage.VSAbstractions.OutputWindow.ILogger;

namespace Test
{
    internal class TestOperationStateInvocationManager_Tests
    {
        private AutoMoqer mocker;
        private TestOperationStateInvocationManager testOperationStateInvocationManager;

        [SetUp]
        public void SetUp()
        {
            mocker = new AutoMoqer();
            testOperationStateInvocationManager = mocker.Create<TestOperationStateInvocationManager>();
        }

        [Test]
        public async Task Should_Return_True_When_Initialized_And_TestExecutionStarting_Async()
        {
            mocker.GetMock<IInitializeStatusProvider>().Setup(initializeStatusProvider => initializeStatusProvider.InitializeStatus).Returns(InitializeStatus.Initialized);
            Assert.That(await testOperationStateInvocationManager.CanInvokeAsync(TestOperationStates.TestExecutionStarting), Is.True);
        }

        [Test]
        public async Task Should_Return_False_When_Not_Initialized_And_TestExecutionStarting_Async()
        {
            mocker.GetMock<IInitializeStatusProvider>().Setup(initializeStatusProvider => initializeStatusProvider.InitializeStatus).Returns(InitializeStatus.Initializing);
            Assert.That(await testOperationStateInvocationManager.CanInvokeAsync(TestOperationStates.TestExecutionStarting), Is.False);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task Should_Return_True_For_All_Other_States_If_Was_Initialized_When_TestExecutionStarting_Async(bool initializedWhenStarting)
        {
            var startingInitializeStatus = initializedWhenStarting ? InitializeStatus.Initialized : InitializeStatus.Initializing;
            mocker.GetMock<IInitializeStatusProvider>().Setup(initializeStatusProvider => initializeStatusProvider.InitializeStatus).Returns(startingInitializeStatus);
            await testOperationStateInvocationManager.CanInvokeAsync(TestOperationStates.TestExecutionStarting);
            Assert.That(await testOperationStateInvocationManager.CanInvokeAsync(TestOperationStates.TestExecutionCancelAndFinished), Is.EqualTo(initializedWhenStarting));
        }

        [TestCase(TestOperationStates.TestExecutionStarting)]
        [TestCase(TestOperationStates.TestExecutionFinished)]
        public async Task Should_Log_When_Cannot_Invoke_Async(TestOperationStates testOperationState)
        {
            await testOperationStateInvocationManager.CanInvokeAsync(testOperationState);
#pragma warning disable VSTHRD110 // Observe result of async calls
            mocker.Verify<ILogger>(logger => logger.LogAsync($"Skipping {testOperationState} as FCC not initialized"));
#pragma warning restore VSTHRD110 // Observe result of async calls
        }
       
    }

    internal class TestContainerDiscovery_Tests
    {
        private AutoMoqer mocker;
        private TestContainerDiscoverer testContainerDiscoverer;

        private void RaiseOperationStateChanged(TestOperationStates testOperationStates,IOperation operation = null)
        {
            var args = operation == null ? new OperationStateChangedEventArgs(testOperationStates) : new OperationStateChangedEventArgs(operation, (RequestStates)testOperationStates);
            mocker.GetMock<IOperationState>().Raise(s => s.StateChanged += null, args);
        }
        
        private void RaiseTestExecutionStarting(IOperation operation = null)
        {
            RaiseOperationStateChanged(TestOperationStates.TestExecutionStarting,operation);
        }

        private void RaiseTestExecutionFinished(IOperation operation = null)
        {
            RaiseOperationStateChanged(TestOperationStates.TestExecutionFinished,operation);
        }

        private void RaiseTestExecutionCancelling()
        {
            RaiseOperationStateChanged(TestOperationStates.TestExecutionCanceling);
        }

        private void AssertShouldNotCollect()
        {
#pragma warning disable VSTHRD110 // Observe result of async calls
            mocker.Verify<IMsCodeCoverageRunSettingsService>(
                msCodeCoverageRunSettingsService => msCodeCoverageRunSettingsService.CollectAsync(It.IsAny<IOperation>(), It.IsAny<ITestOperation>()),
                Times.Never());
#pragma warning restore VSTHRD110 // Observe result of async calls
        }

        private void SetUpOptions(RunOptions runOptions)
        {
            mocker.GetMock<IOptionsProvider<RunOptions>>().Setup(p => p.Get()).Returns(runOptions);
        }

        private (IOperation operation, List<ICoverageProject> coverageProjects, Mock<ITestOperation> mockTestOperation) SetUpForProceedPath()
        {
            var operation = new Mock<IOperation>().Object;
            var mockTestOperation = new Mock<ITestOperation>();
            var coverageProjects = new List<ICoverageProject>();
            mockTestOperation.Setup(t => t.GetCoverageProjectsAsync()).Returns(Task.FromResult(coverageProjects));
            mocker.GetMock<ITestOperationFactory>().Setup(f => f.Create(operation)).Returns(mockTestOperation.Object);
            return (operation, coverageProjects, mockTestOperation);
        }

        [SetUp]
        public void SetUp()
        {
            mocker = new AutoMoqer();
            mocker.GetMock<ITUnitCoverage>()
                .Setup(tUnitCoverage => tUnitCoverage.PrepareForTestRunAsync())
                .ReturnsAsync(Array.Empty<string>());
            testContainerDiscoverer = mocker.Create<TestContainerDiscoverer>();
            testContainerDiscoverer.RunAsync = (taskProvider) =>
            {
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
                taskProvider().Wait();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
            };
            var mockTestOperationStateInvocationManager = mocker.GetMock<ITestOperationStateInvocationManager>();
            mockTestOperationStateInvocationManager.Setup(testOperationStateInvocationManager => testOperationStateInvocationManager.CanInvokeAsync(It.IsAny<TestOperationStates>())).ReturnsAsync(true);
        }

        [Test]
        public void It_Should_Load_The_Package()
        {
#pragma warning disable VSTHRD110 // Observe result of async calls
            mocker.Verify<IPackageLoader>(packageLoader => packageLoader.LoadPackageAsync(It.IsAny<CancellationToken>()));
#pragma warning restore VSTHRD110 // Observe result of async calls
        }

        [Test]
        public void Should_Stop_Coverage_When_Tests_Are_Cancelled()
        {
            RaiseTestExecutionCancelling();
            mocker.Verify<IFCCEngine>(e => e.StopCoverage());
        }

        [Test]
        public void Should_StopCoverage_When_TestExecutionStarting()
        {
            RaiseTestExecutionStarting();
            mocker.Verify<IFCCEngine>(engine => engine.StopCoverage());
        }

        [Test]
        public void Should_Stop_Ms_CodeCoverage_When_TestExecutionStarting_And_Ms_Code_Coverage_Collecting()
        {
            var mockMsCodeCoverageRunSettingsService = SetMsCodeCoverageCollecting();
            mockMsCodeCoverageRunSettingsService.Verify(
                msCodeCoverageRunSettingsService => msCodeCoverageRunSettingsService.StopCoverage(),
                Times.Never
            );

            RaiseTestExecutionStarting();

            mockMsCodeCoverageRunSettingsService.Verify(
                msCodeCoverageRunSettingsService => msCodeCoverageRunSettingsService.StopCoverage()
            );
        }

        [TestCase(MsCodeCoverageCollectionStatus.Collecting,true)]
        [TestCase(MsCodeCoverageCollectionStatus.NotCollecting,true)]
        [TestCase(MsCodeCoverageCollectionStatus.Error,true)]
        [TestCase(MsCodeCoverageCollectionStatus.Collecting, false)]
        [TestCase(MsCodeCoverageCollectionStatus.NotCollecting, false)]
        [TestCase(MsCodeCoverageCollectionStatus.Error, false)]
#pragma warning disable VSTHRD200 // Use "Async" suffix for async methods
        public void Should_Notify_MsCodeCoverage_When_Test_Execution_Not_Finished_IfCollectingAsync(MsCodeCoverageCollectionStatus status, bool cancelling)
#pragma warning restore VSTHRD200 // Use "Async" suffix for async methods
        {
            var mockMsCodeCoverageRunSettingsService = SetMsCodeCoverageCollecting(status);
            var operation = new Mock<IOperation>().Object;
            var mockTestOperationFactory = mocker.GetMock<ITestOperationFactory>();
            var testOperation = new Mock<ITestOperation>().Object;
            mockTestOperationFactory.Setup(testOperationFactory => testOperationFactory.Create(operation)).Returns(testOperation);

            RaiseOperationStateChanged(
                cancelling ? TestOperationStates.TestExecutionCanceling : TestOperationStates.TestExecutionCancelAndFinished, 
                operation
            );
            var times = status == MsCodeCoverageCollectionStatus.Collecting ? Times.Once() : Times.Never();
            mockMsCodeCoverageRunSettingsService.Verify(
                msCodeCoverageRunSettingsService => msCodeCoverageRunSettingsService.TestExecutionNotFinishedAsync(testOperation), times
            );
        }

        private Mock<IMsCodeCoverageRunSettingsService> SetMsCodeCoverageCollecting(MsCodeCoverageCollectionStatus status = MsCodeCoverageCollectionStatus.Collecting)
        {
            var mockMsCodeCoverageRunSettingsService = mocker.GetMock<IMsCodeCoverageRunSettingsService>();
            mockMsCodeCoverageRunSettingsService.Setup(
                msCodeCoverageRunSettingsService =>
                msCodeCoverageRunSettingsService.IsCollectingAsync(It.IsAny<ITestOperation>(), It.IsAny<IReadOnlyCollection<string>>())
            ).ReturnsAsync(status);

            SetUpOptions(new RunOptions { Enabled = true});
            RaiseTestExecutionStarting();
            return mockMsCodeCoverageRunSettingsService;
        }


        [Test]
        public void Should_Collect_Ms_Code_Coverage_When_TestExecutionFinished_And_Ms_Code_Coverage_Collecting()
        {
            SetMsCodeCoverageCollecting();

            var operation = new Mock<IOperation>().Object;
            var testOperation = new Mock<ITestOperation>().Object;
            var mockTestOperationFactory = mocker.GetMock<ITestOperationFactory>();
            mockTestOperationFactory.Setup(testOperationFactory => testOperationFactory.Create(operation)).Returns(testOperation);

            RaiseTestExecutionFinished(operation);
#pragma warning disable VSTHRD110 // Observe result of async calls
            mocker.Verify<IMsCodeCoverageRunSettingsService>(
                msCodeCoverageRunSettingsService =>
                msCodeCoverageRunSettingsService.CollectAsync(operation,testOperation)
            );
#pragma warning restore VSTHRD110 // Observe result of async calls
        }

        [TestCase(true, 10, 1, 0, true, Description = "Should collect when tests fail if settings RunWhenTestsFail is true")]
        [TestCase(false, 10, 1, 0, false, Description = "Should not collect when tests fail if settings RunWhenTestsFail is false")]
        [TestCase(false, 0, 1, 1, false, Description = "Should not collect when total tests does not exceed the RunWhenTestsExceed setting")]
        [TestCase(false, 0, 1, 0, true, Description = "Should collect when total tests does not exceed the RunWhenTestsExceed setting")]
        public void Conditional_Collect_Coverage_When_TestExecutionFinished(bool runWhenTestsFail, long numberFailedTests, long totalTests, int runWhenTestsExceed, bool expectCollect)
        {
            var operation = new Mock<IOperation>().Object;
            var mockTestOperation = new Mock<ITestOperation>();
            mockTestOperation.Setup(o => o.FailedTests).Returns(numberFailedTests);
            mockTestOperation.Setup(o => o.TotalTests).Returns(totalTests);
            var testOperation = mockTestOperation.Object;
            mocker.GetMock<ITestOperationFactory>().Setup(f => f.Create(operation)).Returns(testOperation);

            var mockMsCodeCoverageRunSettingsService = mocker.GetMock<IMsCodeCoverageRunSettingsService>();
            mockMsCodeCoverageRunSettingsService.Setup(s => s.IsCollectingAsync(It.IsAny<ITestOperation>(), It.IsAny<IReadOnlyCollection<string>>())).ReturnsAsync(MsCodeCoverageCollectionStatus.Collecting);

            SetUpOptions(new RunOptions { Enabled = true, RunWhenTestsFail = runWhenTestsFail, RunWhenTestsExceed = runWhenTestsExceed });
            RaiseTestExecutionStarting(operation);
            RaiseTestExecutionFinished(operation);

            var times = expectCollect ? Times.Once() : Times.Never();
#pragma warning disable VSTHRD110 // Observe result of async calls
            mockMsCodeCoverageRunSettingsService.Verify(s => s.CollectAsync(operation, testOperation), times);
#pragma warning restore VSTHRD110 // Observe result of async calls
        }

        [Test]
        public void Should_Not_Collect_Coverage_When_TestExecutionFinished_If_Enabled_Is_False()
        {
            var operation = new Mock<IOperation>().Object;
            var mockTestOperation = new Mock<ITestOperation>();
            mockTestOperation.Setup(t => t.TotalTests).Returns(1);
            mocker.GetMock<ITestOperationFactory>().Setup(f => f.Create(operation)).Returns(mockTestOperation.Object);

            SetUpOptions(new RunOptions { Enabled = false, RunWhenTestsFail = true, RunWhenTestsExceed = 0 });

            RaiseTestExecutionFinished();

            AssertShouldNotCollect();
        }

        [Test]
        public void Should_Handle_Any_Exception_In_OperationState_Changed_Handler_Logging_The_Exception()
        {
            var exception = new Exception("msg");
            mocker.GetMock<IFCCEngine>().Setup(engine => engine.StopCoverage()).Throws(exception);
            RaiseTestExecutionCancelling();
#pragma warning disable VSTHRD110 // Observe result of async calls
            mocker.Verify<ILogger>(logger => logger.LogAsync("Error processing unit test events", exception.ToString()));
#pragma warning restore VSTHRD110 // Observe result of async calls
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Should_Not_Handle_OperationState_Changes_When_The_testOperationStateInvocationManager_Cannot_Invoke(bool canInvoke)
        {
            var invoked = false;
            testContainerDiscoverer.TestOperationStateChangeHandlers = new Dictionary<TestOperationStates, Func<IOperation, Task>>
            {
                {TestOperationStates.TestExecutionCanceling, (_) => {invoked = true; return Task.CompletedTask; } }
            };
            var mockTestOperationStateInvocationManager = mocker.GetMock<ITestOperationStateInvocationManager>();
            mockTestOperationStateInvocationManager.Setup(testOperationStateInvocationManager => testOperationStateInvocationManager.CanInvokeAsync(It.IsAny<TestOperationStates>())).ReturnsAsync(canInvoke);
           
            RaiseTestExecutionCancelling();
            Assert.That(invoked, Is.EqualTo(canInvoke));
        }

        [Test]
        public void Should_Send_TestExecutionStartingMessage_When_TestExecutionStarting()
        {
            var operation = new Mock<IOperation>().Object;
            RaiseTestExecutionStarting(operation);
            mocker.Verify<IEventAggregator>(eventAggregator => eventAggregator.SendMessage(It.IsAny<TestExecutionStartingMessage>(),null));
        }

        [Test]
        public void Should_MsCodeCoverageRunSettingsService_TestExecutionNotFinishedAsync_When_IsCollecting_TestExecutionFinishedAsync_And_CollectAsync_Not_Called()
        {
            var mockMsCodeCoverageRunSettingsService = mocker.GetMock<IMsCodeCoverageRunSettingsService>();
            mockMsCodeCoverageRunSettingsService.Setup(
                msCodeCoverageRunSettingsService =>
                msCodeCoverageRunSettingsService.IsCollectingAsync(It.IsAny<ITestOperation>(), It.IsAny<IReadOnlyCollection<string>>())
            ).ReturnsAsync(MsCodeCoverageCollectionStatus.Collecting);

            SetUpOptions(new RunOptions { Enabled = true, RunWhenTestsFail = false });

            var mockTestOperation = new Mock<ITestOperation>();
                mockTestOperation.SetupGet(testOperation => testOperation.FailedTests).Returns(1);
            mocker.GetMock<ITestOperationFactory>().Setup(f => f.Create(It.IsAny<IOperation>())).Returns(mockTestOperation.Object);
            RaiseTestExecutionStarting();
            RaiseTestExecutionFinished();

            mockMsCodeCoverageRunSettingsService.Verify(msCodeCoverageRunSettingsService => msCodeCoverageRunSettingsService.TestExecutionNotFinishedAsync(mockTestOperation.Object));

        }

        [Test]
        public void Should_Log_Coverage_Starting_With_Run_Number_When_TestExecutionStartingAsync_And_Coverage_Not_Disabled()
        {
            SetUpOptions(new RunOptions { Enabled = true });

            var operation = new Mock<IOperation>().Object;
            RaiseTestExecutionStarting(operation);

            VerifyAsyncLog("================================== COVERAGE STARTING - 1 ==================================");

            RaiseTestExecutionStarting(operation);

            VerifyAsyncLog("================================== COVERAGE STARTING - 2 ==================================");
        }

        private void VerifyAsyncLog(string message)
        {
#pragma warning disable VSTHRD110 // Observe result of async calls
            mocker.Verify<ILogger>(logger => logger.LogAsync(message));
#pragma warning restore VSTHRD110 // Observe result of async calls
        }

        [Test]
        public void Should_Not_Log_Coverage_Starting_When_Coverage_Disabled()
        {
            SetUpOptions(new RunOptions { Enabled = false, DisabledNoCoverage = true });

            var operation = new Mock<IOperation>().Object;
            RaiseTestExecutionStarting(operation);

            mocker.Verify<ILogger>(
                logger => logger.LogFileAndForget("================================== COVERAGE STARTING - 1 =================================="), Times.Never());

        }

    }

    /*
        A solution can mix classic VSTest projects with Microsoft.Testing.Platform projects.  Both
        collectors run and their coverage is combined into a single report, so that every project in
        the run is covered.
    */
    internal class TestContainerDiscovery_Mixed_Solution_Tests
    {
        private const string TestingPlatformProjectFilePath = @"C:\Solution\MtpTests\MtpTests.csproj";

        private AutoMoqer mocker;
        private ICoverageProject vsTestCoverageProject;
        private ICoverageProject testingPlatformCoverageProject;

        [SetUp]
        public void SetUp()
        {
            mocker = new AutoMoqer();
            vsTestCoverageProject = new Mock<ICoverageProject>().Object;
            testingPlatformCoverageProject = new Mock<ICoverageProject>().Object;

            mocker.GetMock<ITestOperationStateInvocationManager>()
                .Setup(testOperationStateInvocationManager => testOperationStateInvocationManager.CanInvokeAsync(It.IsAny<TestOperationStates>())).ReturnsAsync(true);
            mocker.GetMock<IOptionsProvider<RunOptions>>()
                .Setup(runOptionsProvider => runOptionsProvider.Get())
                .Returns(new RunOptions { Enabled = true, RunWhenTestsFail = true, CollectTestingPlatformCoverageAfterTestRun = true });

            var mockTestOperation = new Mock<ITestOperation>();
            mockTestOperation.Setup(testOperation => testOperation.GetCoverageProjectsAsync()).ReturnsAsync(new List<ICoverageProject>());
            mocker.GetMock<ITestOperationFactory>()
                .Setup(testOperationFactory => testOperationFactory.Create(It.IsAny<IOperation>())).Returns(mockTestOperation.Object);

            TestingPlatformProjects(TestingPlatformProjectFilePath);
            TestingPlatformCoverage("mtp.cobertura.xml");
            MsCodeCoverage(MsCodeCoverageCollectionStatus.Collecting, "vstest.cobertura.xml");
        }

        private void TestingPlatformProjects(params string[] projectFilePaths)
            => mocker.GetMock<ITUnitCoverage>()
                .Setup(tUnitCoverage => tUnitCoverage.PrepareForTestRunAsync()).ReturnsAsync(projectFilePaths);

        private void TestingPlatformCoverage(params string[] coberturaFiles)
            => mocker.GetMock<ITUnitCoverage>()
                .Setup(tUnitCoverage => tUnitCoverage.CollectAfterTestRunAsync())
                .ReturnsAsync(new CoverageCollectionResult(coberturaFiles, new[] { testingPlatformCoverageProject }));

        private void MsCodeCoverage(MsCodeCoverageCollectionStatus status, params string[] coberturaFiles)
        {
            var mockMsCodeCoverage = mocker.GetMock<IMsCodeCoverageRunSettingsService>();
            mockMsCodeCoverage
                .Setup(msCodeCoverage => msCodeCoverage.IsCollectingAsync(It.IsAny<ITestOperation>(), It.IsAny<IReadOnlyCollection<string>>()))
                .ReturnsAsync(status);
            mockMsCodeCoverage
                .Setup(msCodeCoverage => msCodeCoverage.CollectAsync(It.IsAny<IOperation>(), It.IsAny<ITestOperation>()))
                .ReturnsAsync(new CoverageCollectionResult(coberturaFiles, new[] { vsTestCoverageProject }));
        }

        private void RunTests()
        {
            var testContainerDiscoverer = mocker.Create<TestContainerDiscoverer>();
            testContainerDiscoverer.RunAsync = (taskProvider) =>
            {
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
                taskProvider().Wait();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
            };

            var mockOperationState = mocker.GetMock<IOperationState>();
            mockOperationState.Raise(
                operationState => operationState.StateChanged += null,
                new OperationStateChangedEventArgs(TestOperationStates.TestExecutionStarting));
            mockOperationState.Raise(
                operationState => operationState.StateChanged += null,
                new OperationStateChangedEventArgs(TestOperationStates.TestExecutionFinished));
        }

        [Test]
        public void Should_Run_A_Single_Report_With_The_Coverage_From_Both_Collectors()
        {
            RunTests();

            mocker.Verify<IFCCEngine>(fccEngine => fccEngine.RunAndProcessReport(
                It.Is<CoverageCollectionResult>(result =>
                    result.CoberturaFiles.Count == 2 &&
                    result.CoberturaFiles.Contains("vstest.cobertura.xml") &&
                    result.CoberturaFiles.Contains("mtp.cobertura.xml") &&
                    result.CoverageProjects.Count == 2 &&
                    result.CoverageProjects.Contains(vsTestCoverageProject) &&
                    result.CoverageProjects.Contains(testingPlatformCoverageProject)),
                null), Times.Once());
        }

        [Test]
        public void Should_Keep_Testing_Platform_Projects_Away_From_The_VsTest_Collector()
        {
            RunTests();

#pragma warning disable VSTHRD110 // Observe result of async calls
            mocker.Verify<IMsCodeCoverageRunSettingsService>(msCodeCoverage => msCodeCoverage.IsCollectingAsync(
                It.IsAny<ITestOperation>(),
                It.Is<IReadOnlyCollection<string>>(excluded => excluded.Single() == TestingPlatformProjectFilePath)));
#pragma warning restore VSTHRD110 // Observe result of async calls
        }

        [Test]
        public void Should_Report_When_Only_The_Testing_Platform_Collected()
        {
            MsCodeCoverage(MsCodeCoverageCollectionStatus.NotCollecting);

            RunTests();

            mocker.Verify<IFCCEngine>(fccEngine => fccEngine.RunAndProcessReport(
                It.Is<CoverageCollectionResult>(result => result.CoberturaFiles.Single() == "mtp.cobertura.xml"),
                null), Times.Once());
        }

        [Test]
        public void Should_Report_When_Only_The_VsTest_Collector_Collected()
        {
            TestingPlatformProjects();

            RunTests();

            mocker.Verify<IFCCEngine>(fccEngine => fccEngine.RunAndProcessReport(
                It.Is<CoverageCollectionResult>(result => result.CoberturaFiles.Single() == "vstest.cobertura.xml"),
                null), Times.Once());
        }

        [Test]
        public void Should_Not_Collect_For_The_Testing_Platform_When_There_Are_No_Such_Projects()
        {
            TestingPlatformProjects();

            RunTests();

#pragma warning disable VSTHRD110 // Observe result of async calls
            mocker.Verify<ITUnitCoverage>(tUnitCoverage => tUnitCoverage.CollectAfterTestRunAsync(), Times.Never());
#pragma warning restore VSTHRD110 // Observe result of async calls
        }

        [Test]
        public void Should_Not_Prepare_The_Testing_Platform_When_The_Option_Is_Off()
        {
            mocker.GetMock<IOptionsProvider<RunOptions>>()
                .Setup(runOptionsProvider => runOptionsProvider.Get())
                .Returns(new RunOptions { Enabled = true, RunWhenTestsFail = true, CollectTestingPlatformCoverageAfterTestRun = false });

            RunTests();

#pragma warning disable VSTHRD110 // Observe result of async calls
            mocker.Verify<ITUnitCoverage>(tUnitCoverage => tUnitCoverage.PrepareForTestRunAsync(), Times.Never());
            mocker.Verify<ITUnitCoverage>(tUnitCoverage => tUnitCoverage.CollectAfterTestRunAsync(), Times.Never());
#pragma warning restore VSTHRD110 // Observe result of async calls
        }

        [Test]
        public void Should_End_Coverage_When_Neither_Collector_Produced_Anything()
        {
            MsCodeCoverage(MsCodeCoverageCollectionStatus.NotCollecting);
            TestingPlatformCoverage();

            RunTests();

            mocker.Verify<IFCCEngine>(fccEngine => fccEngine.RunAndProcessReport(
                It.IsAny<CoverageCollectionResult>(), null), Times.Never());
            mocker.Verify<IEventAggregator>(
                eventAggregator => eventAggregator.SendMessage(It.Is<EmptyCoverageMessage>(message => !message.Collection.HasCoverage), null), Times.Once());
            mocker.Verify<IEventAggregator>(
                eventAggregator => eventAggregator.SendMessage(It.IsAny<CoverageEndedMessage>(), null), Times.AtLeastOnce());
        }
    }
}
