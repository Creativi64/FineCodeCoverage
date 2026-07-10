using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Collection.Engine;
using FineCodeCoverage.Collection.Messages;
using FineCodeCoverage.Collection.Ms;
using FineCodeCoverage.Collection.TestExplorer;
using FineCodeCoverage.Initialization;
using FineCodeCoverage.Options.Base;
using FineCodeCoverage.Options.Run;
using FineCodeCoverage.Utilities.Events;
using FineCodeCoverage.Utilities.Logging;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using Microsoft.VisualStudio.Utilities;
using ILogger = FineCodeCoverage.VSAbstractions.OutputWindow.ILogger;
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Collection.Runners
{
    [Name("FineCodeCoverage.TestContainerDiscoverer")]

    // Both exports necessary !
    [Export(typeof(TestContainerDiscoverer))]
    [Export(typeof(ITestContainerDiscoverer))]
    internal sealed class TestContainerDiscoverer : ITestContainerDiscoverer
    {
#pragma warning disable 67
        public event EventHandler TestContainersUpdated;
#pragma warning restore 67

        private readonly IFCCEngine _fccEngine;
        private readonly ITestOperationStateInvocationManager _testOperationStateInvocationManager;
        private readonly ITestOperationFactory _testOperationFactory;
        private readonly ILogger _logger;
        private readonly IOptionsProvider<RunOptions> _runOptionsProvider;
        private readonly IMsCodeCoverageRunSettingsService _msCodeCoverageRunSettingsService;
        private readonly IEventAggregator _eventAggregator;
        private readonly ICoverageCollectableFromTestExplorer _coverageCollectableFromTestExplorer;
        private readonly ITUnitCoverage _tUnitCoverage;
        private bool _cancelling;
        private MsCodeCoverageCollectionStatus _msCodeCoverageCollectionStatus;
        private RunOptions _runOptions;
        private int _coverageRunNumber = 1;

        [ExcludeFromCodeCoverage]
        public Uri ExecutorUri => new Uri("executor://FineCodeCoverage.Executor/v1");

        [ExcludeFromCodeCoverage]
        public IEnumerable<ITestContainer> TestContainers => Enumerable.Empty<ITestContainer>();

        public bool MsCodeCoverageErrored => _msCodeCoverageCollectionStatus == MsCodeCoverageCollectionStatus.Error;

        // internal properties for tests
        internal Dictionary<TestOperationStates, Func<IOperation, Task>> TestOperationStateChangeHandlers { get; set; }

        internal Task InitializeTask { get; set; }

        internal Action<Func<Task>> RunAsync { get; set; } = (taskProvider) => ThreadHelper.JoinableTaskFactory.Run(taskProvider);

        [ImportingConstructor]
        public TestContainerDiscoverer(
            [Import(typeof(IOperationState))]
            IOperationState operationState,
            IFCCEngine fccEngine,
            ITestOperationStateInvocationManager testOperationStateInvocationManager,
            IPackageLoader packageLoader,
            ITestOperationFactory testOperationFactory,
            ILogger logger,
            IOptionsProvider<RunOptions> runOptionsProvider,
            IMsCodeCoverageRunSettingsService msCodeCoverageRunSettingsService,
            IEventAggregator eventAggregator,
            ICoverageCollectableFromTestExplorer coverageCollectableFromTestExplorer,
            ITUnitCoverage tUnitCoverage)
        {
            _runOptionsProvider = runOptionsProvider;
            _msCodeCoverageRunSettingsService = msCodeCoverageRunSettingsService;
            _eventAggregator = eventAggregator;
            _coverageCollectableFromTestExplorer = coverageCollectableFromTestExplorer;
            _tUnitCoverage = tUnitCoverage;
            _fccEngine = fccEngine;
            _testOperationStateInvocationManager = testOperationStateInvocationManager;
            _testOperationFactory = testOperationFactory;
            _logger = logger;
            TestOperationStateChangeHandlers = new Dictionary<TestOperationStates, Func<IOperation, Task>>
            {
                { TestOperationStates.TestExecutionCanceling, TestExecutionCancellingAsync },
                { TestOperationStates.TestExecutionStarting, TestExecutionStartingAsync },
                { TestOperationStates.TestExecutionFinished, TestExecutionFinishedAsync },
                { TestOperationStates.TestExecutionCancelAndFinished, TestExecutionCancelAndFinishedAsync },
            };
            _ = packageLoader.LoadPackageAsync(CancellationToken.None);
            operationState.StateChanged += OperationState_StateChanged;
        }

        private static bool CoverageDisabled(RunOptions runOptions)
            => !runOptions.Enabled && runOptions.DisabledNoCoverage;

        private Task LogCoverageStartingAsync()
            => _logger.LogAsync(StatusMarkerProvider.Get($"Coverage Starting - {_coverageRunNumber++}"));

        private async Task TestExecutionStartingAsync(IOperation operation)
        {
            _eventAggregator.SendMessage(new TestExecutionStartingMessage());
            _cancelling = false;
            StopCoverage();

            RunOptions settings = _runOptionsProvider.Get();
            if (CoverageDisabled(settings))
            {
                await _logger.LogAsync("Coverage not collected as FCC disabled.");
                RaiseCoverageEnded();
                return;
            }

            await LogCoverageStartingAsync();
            _msCodeCoverageCollectionStatus = await _msCodeCoverageRunSettingsService.IsCollectingAsync(
                _testOperationFactory.Create(operation));
            if (_msCodeCoverageCollectionStatus == MsCodeCoverageCollectionStatus.NotCollecting)
            {
                await _logger.LogAsync("Ms code coverage not collecting for this run.");
            }

            if (_msCodeCoverageCollectionStatus != MsCodeCoverageCollectionStatus.Collecting)
            {
                return;
            }

            RaiseCoverageStarted();
        }

        private async Task TestExecutionFinishedAsync(IOperation operation)
        {
            (bool should, ITestOperation testOperation) = await ShouldConditionallyCollectWhenTestExecutionFinishedAsync(operation);
            if (should)
            {
                await TestExecutionFinishedCollectionAsync(operation, testOperation);
            }
            else
            {
                if (_msCodeCoverageCollectionStatus == MsCodeCoverageCollectionStatus.Collecting)
                {
                    await _msCodeCoverageRunSettingsService.TestExecutionNotFinishedAsync(testOperation);
                }
            }
        }

        private async Task<(bool should, ITestOperation testOperation)> ShouldConditionallyCollectWhenTestExecutionFinishedAsync(
            IOperation operation)
        {
            if (ShouldNotCollectWhenTestExecutionFinished())
            {
                return (false, null);
            }

            ITestOperation testOperation = _testOperationFactory.Create(operation);

            bool shouldCollect = await CoverageConditionsMetAsync(testOperation);
            return (shouldCollect, testOperation);
        }

        private bool ShouldNotCollectWhenTestExecutionFinished()
        {
            _runOptions = _runOptionsProvider.Get();
            return CoverageDisabled(_runOptions) || MsCodeCoverageErrored;
        }

        private async Task TestExecutionFinishedCollectionAsync(IOperation operation, ITestOperation testOperation)
        {
            if (_msCodeCoverageCollectionStatus == MsCodeCoverageCollectionStatus.Collecting)
            {
                await _msCodeCoverageRunSettingsService.CollectAsync(operation, testOperation);
            }
        }

        private async Task<bool> CoverageConditionsMetAsync(ITestOperation testOperation)
        {
            if (!_runOptions.RunWhenTestsFail && testOperation.FailedTests > 0)
            {
                await _logger.LogAsync($"Skipping coverage due to failed tests.  Option {nameof(RunOptions.RunWhenTestsFail)} is false");
                RaiseCoverageEnded();
                return false;
            }

            long totalTests = testOperation.TotalTests;
            int runWhenTestsExceed = _runOptions.RunWhenTestsExceed;

            // in case this changes to not reporting total tests
            if (totalTests <= 0 || totalTests > runWhenTestsExceed)
            {
                return true;
            }

            await _logger.LogAsync($"Skipping coverage as total tests ({totalTests}) <= {nameof(RunOptions.RunWhenTestsExceed)} ({runWhenTestsExceed})");
            RaiseCoverageEnded();
            return false;
        }

        private void StopCoverage()
        {
            if (_msCodeCoverageCollectionStatus == MsCodeCoverageCollectionStatus.Collecting)
            {
                _msCodeCoverageRunSettingsService.StopCoverage();
            }

            // Cancel any in-flight engine work (e.g. report generation from a previous run) so a new test
            // run starts from a clean slate.  Idempotent: no-op when nothing is running.
            _fccEngine.StopCoverage();
        }

        private async Task CoverageCancelledAsync(string logMessage, IOperation operation)
        {
            await _logger.LogAsync(logMessage);
            RaiseCoverageEnded();
            _fccEngine.StopCoverage();
            await NotifyMsCodeCoverageTestExecutionNotFinishedIfCollectingAsync(operation);
        }

        private async Task NotifyMsCodeCoverageTestExecutionNotFinishedIfCollectingAsync(IOperation operation)
        {
            if (_msCodeCoverageCollectionStatus != MsCodeCoverageCollectionStatus.Collecting)
            {
                return;
            }

            ITestOperation testOperation = _testOperationFactory.Create(operation);
            await _msCodeCoverageRunSettingsService.TestExecutionNotFinishedAsync(testOperation);
        }

        private void OperationState_StateChanged(object sender, OperationStateChangedEventArgs e)
            => RunAsync(async () => await TryAndLogExceptionAsync(() => OperationState_StateChangedAsync(e)));

        private async Task TestExecutionCancellingAsync(IOperation operation)
        {
            _cancelling = true;
            await CoverageCancelledAsync("Test execution cancelling - running coverage will be cancelled.", operation);
        }

        private async Task TestExecutionCancelAndFinishedAsync(IOperation operation)
        {
            if (_cancelling)
            {
                return;
            }

            await CoverageCancelledAsync("There has been an issue running tests. See the Tests output window pane.", operation);
        }

        private async Task OperationState_StateChangedAsync(OperationStateChangedEventArgs e)
        {
            if (!TestOperationStateChangeHandlers.TryGetValue(e.State, out Func<IOperation, Task> handler)
                || !await _testOperationStateInvocationManager.CanInvokeAsync(e.State))
            {
                return;
            }

            await _logger.LogAsync($"Test operation state - {e.State}.");

            if (!await _coverageCollectableFromTestExplorer.IsCollectableAsync())
            {
                // Microsoft.Testing.Platform projects are present.  They run on MTP, which produces no
                // VSTest data-collector coverage, so the classic Test Explorer collection cannot work.
                // Optionally re-run the test hosts under --coverage once the test run has finished.
                if (e.State == TestOperationStates.TestExecutionStarting)
                {
                    await _logger.LogAsync("Coverage is not collectable from the test explorer as Microsoft.Testing.Platform projects are present.");
                }

                if (e.State == TestOperationStates.TestExecutionFinished
                    && _runOptionsProvider.Get().CollectTestingPlatformCoverageAfterTestRun)
                {
                    _tUnitCoverage.CollectCoverage();
                }

                return;
            }

            await handler(e.Operation);
        }

        private async Task TryAndLogExceptionAsync(Func<Task> action)
        {
            try
            {
                await action();
            }
            catch (Exception exception)
            {
                await _logger.LogAsync("Error processing unit test events", exception.ToString());
            }
        }

        private void RaiseCoverageStarted(bool pending = false)
            => _eventAggregator.SendMessage(new CoverageStartingMessage(pending));

        private void RaiseCoverageEnded() => _eventAggregator.SendMessage(new CoverageEndedMessage());
    }
}
