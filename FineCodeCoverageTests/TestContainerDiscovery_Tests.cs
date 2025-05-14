using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using AutoMoq;
using FineCodeCoverage.Core.Initialization;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage;
using FineCodeCoverage.Impl;
using FineCodeCoverage.Impl.TestContainerDiscovery;
using FineCodeCoverage.Options;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using Moq;
using NUnit.Framework;
using ILogger = FineCodeCoverage.Output.ILogger;

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

        private void AssertShouldNotReloadCoverage()
        {
            mocker.Verify<IFCCEngine>(engine => engine.ReloadCoverage(It.IsAny<Func<Task<List<ICoverageProject>>>>()), Times.Never());
        }

        private void AssertReloadsCoverage()
        {
            mocker.Verify<IFCCEngine>(engine => engine.ReloadCoverage(It.IsAny<Func<Task<List<ICoverageProject>>>>()), Times.Once());
        }

        private void SetUpOptions(AppOptions appOptions)
        {
            mocker.GetMock<IAppOptionsProvider>().Setup(appOptionsProvider => appOptionsProvider.Get()).Returns(appOptions);
        }

        private (IOperation operation, List<ICoverageProject> coverageProjects, Mock<ITestOperation> mockTestOperation) SetUpForProceedPath()
        {
            var operation = new Mock<IOperation>().Object;
            var mockTestOperation = new Mock<ITestOperation>();
            var coverageProjects = new List<ICoverageProject>();
            mockTestOperation.Setup(t => t.GetCoverageProjectsAsync()).Returns(Task.FromResult(coverageProjects));
            mocker.GetMock<ITestOperationFactory>().Setup(f => f.CreateAsync(operation)).ReturnsAsync(mockTestOperation.Object);
            return (operation, coverageProjects, mockTestOperation);

        }

        [SetUp]
        public void SetUp()
        {
            mocker = new AutoMoqer();
            var mockCoverageCollectableFromTestExplorer = mocker.GetMock<ICoverageCollectableFromTestExplorer>();
            mockCoverageCollectableFromTestExplorer.Setup(coverageCollectableFromTestExplorer => coverageCollectableFromTestExplorer.IsCollectableAsync()).ReturnsAsync(true);
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
            mockTestOperationFactory.Setup(testOperationFactory => testOperationFactory.CreateAsync(operation)).ReturnsAsync(testOperation);

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
                msCodeCoverageRunSettingsService.IsCollectingAsync(It.IsAny<ITestOperation>())
            ).ReturnsAsync(status);

            SetUpOptions(new AppOptions { Enabled = true});
            RaiseTestExecutionStarting();
            return mockMsCodeCoverageRunSettingsService;
        }


        [Test]
        public void Should_Not_ReloadCoverage_When_TestExecutionStarting_And_Settings_RunInParallel_Is_False()
        {
            SetUpOptions(new AppOptions { Enabled = true, RunInParallel = false });
           
            RaiseTestExecutionStarting();

            AssertShouldNotReloadCoverage();
        }

        [Test]
        public void Should_Not_ReloadCoverage_When_TestExecutionFinished_And_Reloading_When_Tests_Start()
        {
            SetUpOptions(new AppOptions { Enabled = true, RunInParallel = true });

            RaiseTestExecutionFinished();

            AssertShouldNotReloadCoverage();
        }

        [Test]
        public void Should_ReloadCoverage_When_TestExecutionFinished_If_RunInParallel_And_Not_Collecting_With_MsCodeCoverage()
        {
            var (operation,_,__) = SetUpForProceedPath();
            SetUpOptions(new AppOptions { 
                Enabled = true,
                RunInParallel = true,
                RunMsCodeCoverage = RunMsCodeCoverage.Yes
            });

            RaiseTestExecutionFinished(operation);

            mocker.Verify<IFCCEngine>(engine => engine.ReloadCoverage(It.IsAny<Func<Task<List<ICoverageProject>>>>()));
        }

        [Test]
        public void Should_Collect_Ms_Code_Coverage_When_TestExecutionFinished_And_Ms_Code_Coverage_Collecting()
        {
            SetMsCodeCoverageCollecting();

            var operation = new Mock<IOperation>().Object;
            var testOperation = new Mock<ITestOperation>().Object;
            var mockTestOperationFactory = mocker.GetMock<ITestOperationFactory>();
            mockTestOperationFactory.Setup(testOperationFactory => testOperationFactory.CreateAsync(operation)).ReturnsAsync(testOperation);

            RaiseTestExecutionFinished(operation);
#pragma warning disable VSTHRD110 // Observe result of async calls
            mocker.Verify<IMsCodeCoverageRunSettingsService>(
                msCodeCoverageRunSettingsService =>
                msCodeCoverageRunSettingsService.CollectAsync(operation,testOperation)
            );
#pragma warning restore VSTHRD110 // Observe result of async calls
        }

        [Test]
        public async Task Should_ReloadCoverage_When_TestExecutionStarting_And_Settings_RunInParallel_Is_True_Async()
        {
            SetUpOptions(new AppOptions { Enabled = true, RunInParallel = true });
            var (operation, coverageProjects, mockTestOperation) = SetUpForProceedPath();
            Task<List<ICoverageProject>> reloadCoverageTask = null;
            mocker.GetMock<IFCCEngine>().Setup(engine => engine.ReloadCoverage(It.IsAny<Func<Task<List<ICoverageProject>>>>())).
                Callback<Func<Task<List<ICoverageProject>>>>(callback =>
                {
                    reloadCoverageTask = callback();
                });
            RaiseTestExecutionStarting(operation);
            Assert.AreSame(coverageProjects, await reloadCoverageTask);
        }

        [Test]
        public void Should_Not_ReloadCoverage_When_TestExecutionStarting_And_Settings_RunInParallel_Is_True_When_Enabled_Is_False_And_DisabledNoCoverage_True()
        {
            SetUpOptions(new AppOptions { Enabled = false, RunInParallel = true, DisabledNoCoverage = true });

            RaiseTestExecutionStarting();

            AssertShouldNotReloadCoverage();
        }

        [Test]
        public void Should_ReloadCoverage_When_TestExecutionStarting_And_Settings_RunInParallel_Is_True_When_Enabled_Is_False_And_DisabledNoCoverage_False()
        {
            SetUpOptions(new AppOptions { Enabled = false, RunInParallel = true, DisabledNoCoverage = false });

            RaiseTestExecutionStarting();

            AssertReloadsCoverage();
        }

        [TestCase(true, 10, 1, 0, true, Description = "Should run when tests fail if settings RunWhenTestsFail is true")]
        [TestCase(false, 10, 1, 0, false, Description = "Should not run when tests fail if settings RunWhenTestsFail is false")]
        [TestCase(false, 0, 1, 1, false, Description = "Should not run when total tests does not exceed the RunWhenTestsExceed setting")]
        [TestCase(false, 0, 1, 0, true, Description = "Should run when total tests does not exceed the RunWhenTestsExceed setting")]
        public async Task Conditional_Run_Coverage_When_TestExecutionFinished_Async(bool runWhenTestsFail, long numberFailedTests, long totalTests, int runWhenTestsExceed, bool expectReloadedCoverage)
        {
            var (operation, coverageProjects, mockTestOperation) = SetUpForProceedPath();
            mockTestOperation.Setup(o => o.FailedTests).Returns(numberFailedTests);
            mockTestOperation.Setup(o => o.TotalTests).Returns(totalTests);
            SetUpOptions(new AppOptions { Enabled = true, RunInParallel = false, RunWhenTestsFail = runWhenTestsFail, RunWhenTestsExceed = runWhenTestsExceed });
            Task<List<ICoverageProject>> reloadCoverageTask = null;
            mocker.GetMock<IFCCEngine>().Setup(engine => engine.ReloadCoverage(It.IsAny<Func<Task<List<ICoverageProject>>>>())).
                Callback<Func<Task<List<ICoverageProject>>>>(callback =>
                {
                    reloadCoverageTask = callback();
                });
            RaiseTestExecutionFinished(operation);

            if (expectReloadedCoverage)
            {
                Assert.AreSame(coverageProjects, await reloadCoverageTask);
            }
            else
            {
                AssertShouldNotReloadCoverage();
            }
            
        }

        [Test]
        public void Should_Not_Run_Coverage_When_TestExecutionFinished_If_Enabled_Is_False()
        {
            var operation = new Mock<IOperation>().Object;
            var mockTestOperation = new Mock<ITestOperation>();
            mockTestOperation.Setup(t => t.TotalTests).Returns(1);
            mocker.GetMock<ITestOperationFactory>().Setup(f => f.CreateAsync(operation)).ReturnsAsync(mockTestOperation.Object);

            SetUpOptions(new AppOptions { Enabled = false, RunWhenTestsFail = true, RunWhenTestsExceed = 0, RunInParallel = false });

            RaiseTestExecutionFinished();

            AssertShouldNotReloadCoverage();
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
            testContainerDiscoverer.testOperationStateChangeHandlers = new Dictionary<TestOperationStates, Func<IOperation, Task>>
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
                msCodeCoverageRunSettingsService.IsCollectingAsync(It.IsAny<ITestOperation>())
            ).ReturnsAsync(MsCodeCoverageCollectionStatus.Collecting);

            SetUpOptions(new AppOptions { Enabled = true, RunWhenTestsFail = false });

            var mockTestOperation = new Mock<ITestOperation>();
                mockTestOperation.SetupGet(testOperation => testOperation.FailedTests).Returns(1);
            mocker.GetMock<ITestOperationFactory>().Setup(f => f.CreateAsync(It.IsAny<IOperation>())).ReturnsAsync(mockTestOperation.Object);
            RaiseTestExecutionStarting();
            RaiseTestExecutionFinished();

            mockMsCodeCoverageRunSettingsService.Verify(msCodeCoverageRunSettingsService => msCodeCoverageRunSettingsService.TestExecutionNotFinishedAsync(mockTestOperation.Object));

        }

        [Test]
        public void Should_Log_Coverage_Starting_With_Run_Number_When_TestExecutionStartingAsync_And_Coverage_Not_Disabled()
        {
            SetUpOptions(new AppOptions { Enabled = true });

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
            SetUpOptions(new AppOptions { Enabled = false, DisabledNoCoverage = true });

            var operation = new Mock<IOperation>().Object;
            RaiseTestExecutionStarting(operation);

            mocker.Verify<ILogger>(
                logger => logger.LogFileAndForget("================================== COVERAGE STARTING - 1 =================================="), Times.Never());

        }

    }

    internal class TestContainerDiscovery_Not_Collectable_Tests
    {
        [Test]
        public void Should_Not_When_Not_Collectable()
        {
            var mocker = new AutoMoqer();
            var mockOperationState = mocker.GetMock<IOperationState>();
            var mockCoverageCollectableFromTestExplorer = mocker.GetMock<ICoverageCollectableFromTestExplorer>();
            mockCoverageCollectableFromTestExplorer.Setup(coverageCollectableFromTestExplorer => coverageCollectableFromTestExplorer.IsCollectableAsync()).ReturnsAsync(false);

            var testContainerDiscoverer = mocker.Create<TestContainerDiscoverer>();
            testContainerDiscoverer.RunAsync = (taskProvider) =>
            {
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
                taskProvider().Wait();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
            };
            var mockTestOperationStateInvocationManager = mocker.GetMock<ITestOperationStateInvocationManager>();
            mockTestOperationStateInvocationManager.Setup(testOperationStateInvocationManager => testOperationStateInvocationManager.CanInvokeAsync(It.IsAny<TestOperationStates>())).ReturnsAsync(true);

            OperationStateChangedEventArgs args = new OperationStateChangedEventArgs(TestOperationStates.TestExecutionStarting);
            mockOperationState.Raise(operationState => operationState.StateChanged += null, args);

            mocker.Verify<IEventAggregator>(ea => ea.SendMessage(It.IsAny<TestExecutionStartingMessage>(), null), Times.Never());
        }
    }
}