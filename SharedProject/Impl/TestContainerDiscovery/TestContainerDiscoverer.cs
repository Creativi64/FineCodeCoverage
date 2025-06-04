using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Core.Initialization;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine;
using FineCodeCoverage.Engine.Messages;
using FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage;
using FineCodeCoverage.Impl.TestContainerDiscovery;
using FineCodeCoverage.Options;
using FineCodeCoverage.Output;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using Microsoft.VisualStudio.Utilities;
using ILogger = FineCodeCoverage.Output.ILogger;
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Impl
{
    [Name(Vsix.TestContainerDiscovererName)]
    // Both exports necessary !
    [Export(typeof(TestContainerDiscoverer))]
    [Export(typeof(ITestContainerDiscoverer))]
    internal class TestContainerDiscoverer : ITestContainerDiscoverer
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
        internal Dictionary<TestOperationStates, Func<IOperation, Task>> testOperationStateChangeHandlers;
        private bool _cancelling;
        private MsCodeCoverageCollectionStatus _msCodeCoverageCollectionStatus;
        private bool _runningInParallel;
        private RunOptions _runOptions;
        private int _coverageRunNumber = 1;

        internal Task initializeTask;

        [ExcludeFromCodeCoverage]
        public Uri ExecutorUri => new Uri($"executor://{Vsix.Code}.Executor/v1");
        [ExcludeFromCodeCoverage]
        public IEnumerable<ITestContainer> TestContainers => Enumerable.Empty<ITestContainer>();
        public bool MsCodeCoverageErrored => this._msCodeCoverageCollectionStatus == MsCodeCoverageCollectionStatus.Error;

        [ImportingConstructor]
        public TestContainerDiscoverer
        (
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
            ICoverageCollectableFromTestExplorer coverageCollectableFromTestExplorer
        )
        {
            this._runOptionsProvider = runOptionsProvider;
            this._msCodeCoverageRunSettingsService = msCodeCoverageRunSettingsService;
            this._eventAggregator = eventAggregator;
            this._coverageCollectableFromTestExplorer = coverageCollectableFromTestExplorer;
            this._fccEngine = fccEngine;
            this._testOperationStateInvocationManager = testOperationStateInvocationManager;
            this._testOperationFactory = testOperationFactory;
            this._logger = logger;
            this.testOperationStateChangeHandlers = new Dictionary<TestOperationStates, Func<IOperation, Task>>
            {
                { TestOperationStates.TestExecutionCanceling, this.TestExecutionCancellingAsync},
                { TestOperationStates.TestExecutionStarting, this.TestExecutionStartingAsync},
                { TestOperationStates.TestExecutionFinished, this.TestExecutionFinishedAsync},
                { TestOperationStates.TestExecutionCancelAndFinished, this.TestExecutionCancelAndFinishedAsync},
            };
            _ = packageLoader.LoadPackageAsync(CancellationToken.None);
            operationState.StateChanged += this.OperationState_StateChanged;
        }

        internal Action<Func<Task>> RunAsync = (taskProvider) => ThreadHelper.JoinableTaskFactory.Run(taskProvider);

        private static bool CoverageDisabled(RunOptions runOptions)
            => !runOptions.Enabled && runOptions.DisabledNoCoverage;

        private Task LogCoverageStartingAsync()
            => this._logger.LogAsync(StatusMarkerProvider.Get($"Coverage Starting - {this._coverageRunNumber++}"));

        private async Task TestExecutionStartingAsync(IOperation operation)
        {
            this._eventAggregator.SendMessage(new TestExecutionStartingMessage());
            this._cancelling = false;
            this._runningInParallel = false;
            this.StopCoverage();

            RunOptions settings = this._runOptionsProvider.Get();
            if (CoverageDisabled(settings))
            {
                await this._logger.LogAsync("Coverage not collected as FCC disabled.");
                this.RaiseCoverageEnded();
                return;
            }

            await this.LogCoverageStartingAsync();
            this._msCodeCoverageCollectionStatus = await this._msCodeCoverageRunSettingsService.IsCollectingAsync(
                this._testOperationFactory.Create(operation));
            if (this._msCodeCoverageCollectionStatus == MsCodeCoverageCollectionStatus.NotCollecting)
            {
                if (settings.RunInParallel)
                {
                    this.RaiseCoverageStarted(true);
                    this._runningInParallel = true;
                    this._fccEngine.ReloadCoverage(async () =>
                    {
                        ITestOperation testOperation = this._testOperationFactory.Create(operation);
                        return await testOperation.GetCoverageProjectsAsync();
                    });
                }
                else
                {
                    this.RaiseCoverageStarted(false);
                    await this._logger.LogAsync("Coverage collected when tests finish. RunInParallel option true for immediate");
                }
            }

            if (this._msCodeCoverageCollectionStatus == MsCodeCoverageCollectionStatus.Collecting)
            {
                this.RaiseCoverageStarted();
            }
        }

        private async Task TestExecutionFinishedAsync(IOperation operation)
        {
            (bool should, ITestOperation testOperation) = await this.ShouldConditionallyCollectWhenTestExecutionFinishedAsync(operation);
            if (should)
            {
                await this.TestExecutionFinishedCollectionAsync(operation, testOperation);
            }
            else
            {
                if (this._msCodeCoverageCollectionStatus == MsCodeCoverageCollectionStatus.Collecting)
                {
                    await this._msCodeCoverageRunSettingsService.TestExecutionNotFinishedAsync(testOperation);
                }
            }
        }

        private async Task<(bool should, ITestOperation testOperation)> ShouldConditionallyCollectWhenTestExecutionFinishedAsync(
            IOperation operation)
        {
            if (this.ShouldNotCollectWhenTestExecutionFinished())
            {
                return (false, null);
            }

            ITestOperation testOperation = this._testOperationFactory.Create(operation);

            bool shouldCollect = await this.CoverageConditionsMetAsync(testOperation);
            return (shouldCollect, testOperation);
        }

        private bool ShouldNotCollectWhenTestExecutionFinished()
        {
            this._runOptions = this._runOptionsProvider.Get();
            return CoverageDisabled(this._runOptions) || this._runningInParallel || this.MsCodeCoverageErrored;
        }

        private async Task TestExecutionFinishedCollectionAsync(IOperation operation, ITestOperation testOperation)
        {
            if (this._msCodeCoverageCollectionStatus == MsCodeCoverageCollectionStatus.Collecting)
            {
                await this._msCodeCoverageRunSettingsService.CollectAsync(operation, testOperation);
            }
            else
            {
                this._fccEngine.ReloadCoverage(testOperation.GetCoverageProjectsAsync);
            }
        }

        private async Task<bool> CoverageConditionsMetAsync(ITestOperation testOperation)
        {
            if (!this._runOptions.RunWhenTestsFail && testOperation.FailedTests > 0)
            {
                await this._logger.LogAsync($"Skipping coverage due to failed tests.  Option {nameof(RunOptions.RunWhenTestsFail)} is false");
                this.RaiseCoverageEnded();
                return false;
            }

            long totalTests = testOperation.TotalTests;
            int runWhenTestsExceed = this._runOptions.RunWhenTestsExceed;
            if (totalTests > 0) // in case this changes to not reporting total tests
            {
                if (totalTests <= runWhenTestsExceed)
                {
                    await this._logger.LogAsync($"Skipping coverage as total tests ({totalTests}) <= {nameof(RunOptions.RunWhenTestsExceed)} ({runWhenTestsExceed})");
                    this.RaiseCoverageEnded();
                    return false;
                }
            }

            return true;
        }

        private void StopCoverage()
        {
            switch (this._msCodeCoverageCollectionStatus)
            {
                case MsCodeCoverageCollectionStatus.Collecting:
                    this._msCodeCoverageRunSettingsService.StopCoverage();
                    break;
                case MsCodeCoverageCollectionStatus.NotCollecting:
                    this._fccEngine.StopCoverage();
                    break;
            }
        }

        private async Task CoverageCancelledAsync(string logMessage, IOperation operation)
        {
            await this._logger.LogAsync(logMessage);
            this.RaiseCoverageEnded();
            this._fccEngine.StopCoverage();
            await this.NotifyMsCodeCoverageTestExecutionNotFinishedIfCollectingAsync(operation);
        }

        private async Task NotifyMsCodeCoverageTestExecutionNotFinishedIfCollectingAsync(IOperation operation)
        {
            if (this._msCodeCoverageCollectionStatus == MsCodeCoverageCollectionStatus.Collecting)
            {
                ITestOperation testOperation = this._testOperationFactory.Create(operation);
                await this._msCodeCoverageRunSettingsService.TestExecutionNotFinishedAsync(testOperation);
            }
        }

        private void OperationState_StateChanged(object sender, OperationStateChangedEventArgs e)
            => this.RunAsync(async () => await this.TryAndLogExceptionAsync(() => this.OperationState_StateChangedAsync(e)));

        private async Task TestExecutionCancellingAsync(IOperation operation)
        {
            this._cancelling = true;
            await this.CoverageCancelledAsync("Test execution cancelling - running coverage will be cancelled.", operation);
        }

        private async Task TestExecutionCancelAndFinishedAsync(IOperation operation)
        {
            if (!this._cancelling)
            {
                await this.CoverageCancelledAsync("There has been an issue running tests. See the Tests output window pane.", operation);
            }
        }

        private async Task OperationState_StateChangedAsync(OperationStateChangedEventArgs e)
        {
            if (this.testOperationStateChangeHandlers.TryGetValue(e.State, out Func<IOperation, Task> handler))
            {
                if (await this._coverageCollectableFromTestExplorer.IsCollectableAsync() && await this._testOperationStateInvocationManager.CanInvokeAsync(e.State))
                {
                    await handler(e.Operation);
                }
            }
        }

        private async Task TryAndLogExceptionAsync(Func<Task> action)
        {
            try
            {
                await action();

            }
            catch (Exception exception)
            {
                await this._logger.LogAsync("Error processing unit test events", exception.ToString());
            }
        }

        private void RaiseCoverageStarted(bool pending = false)
            => this._eventAggregator.SendMessage(new CoverageStartingMessage(pending));

        private void RaiseCoverageEnded() => this._eventAggregator.SendMessage(new CoverageEndedMessage());
    }
}