using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Collection.CoverageProjectManagement;
using FineCodeCoverage.Collection.CoverageToolOutput;
using FineCodeCoverage.Collection.Messages;
using FineCodeCoverage.Collection.ReportGeneration;
using FineCodeCoverage.Utilities.Events;
using FineCodeCoverage.Utilities.Logging;
using FineCodeCoverage.VSAbstractions.OutputWindow;
using FineCodeCoverage.VSAbstractions.Threading;

namespace FineCodeCoverage.Collection.Engine
{
    [Export(typeof(IFCCEngine))]
    internal sealed class FCCEngine : IFCCEngine
    {
        private sealed class ReportResult
        {
            public IReportResult Report { get; set; }

            public string CoberturaFile { get; set; }

            public List<ICoverageProject> CoverageProjects { get; internal set; }

            public CoverageCollectionResult Collection { get; set; }
        }

        internal int InitializeWait { get; set; } = 5000;

        internal const string InitializationFailedMessagePrefix = "Initialization failed.  Please check the following error which may be resolved by reopening visual studio which will start the initialization process again.";

        private readonly IReportGeneratorUtil _reportGeneratorUtil;
        private readonly ILogger _logger;
        private readonly ICoverageToolOutputManager _coverageOutputManager;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDisposeAwareTaskRunner _disposeAwareTaskRunner;
        private readonly IThreadHelper _threadHelper;
        private ICancellationTokenSource _cancellationTokenSource;
        private readonly object _runLock = new object();
        private readonly SemaphoreSlim _reportGate = new SemaphoreSlim(1, 1);

        [ImportingConstructor]
        public FCCEngine(
            IReportGeneratorUtil reportGeneratorUtil,
            ILogger logger,
            ICoverageToolOutputManager coverageOutputManager,
            IEventAggregator eventAggregator,
            IDisposeAwareTaskRunner disposeAwareTaskRunner,
            IThreadHelper threadHelper)
        {
            _eventAggregator = eventAggregator;
            _disposeAwareTaskRunner = disposeAwareTaskRunner;
            _threadHelper = threadHelper;
            _coverageOutputManager = coverageOutputManager;
            _reportGeneratorUtil = reportGeneratorUtil;
            _logger = logger;
        }

        private Task LogCoverageStatusAsync(string reloadCoverageStatus)
            => _logger.LogAsync(StatusMarkerProvider.Get(reloadCoverageStatus));

        public void StopCoverage()
        {
            lock (_runLock)
            {
                _cancellationTokenSource?.Cancel();
            }
        }

        private ICancellationTokenSource Reset()
        {
            lock (_runLock)
            {
                StopCoverage();
                _cancellationTokenSource = _disposeAwareTaskRunner.CreateLinkedTokenSource();
                return _cancellationTokenSource;
            }
        }

        private async Task<ReportResult> RunAndProcessReportAsync(
            string[] coverOutputFiles,
            List<ICoverageProject> coverageProjects,
            CancellationToken vsShutdownLinkedCancellationToken)
        {
            string reportOutputFolder = _coverageOutputManager.GetReportOutputFolder();
            vsShutdownLinkedCancellationToken.ThrowIfCancellationRequested();
            ReportGeneratorResult result = await _reportGeneratorUtil.GenerateAsync(coverOutputFiles, reportOutputFolder, vsShutdownLinkedCancellationToken);

            vsShutdownLinkedCancellationToken.ThrowIfCancellationRequested();
            await _logger.LogAsync("Processing cobertura");

            vsShutdownLinkedCancellationToken.ThrowIfCancellationRequested();

            return new ReportResult
            {
                CoberturaFile = result.UnifiedXmlFile,
                Report = result.ReportResult,
                CoverageProjects = coverageProjects,
            };
        }

        private void RaiseReportFiles(ReportResult reportResult)
        {
            if (reportResult.CoberturaFile == null)
                return;

            _eventAggregator.SendMessage(new ReportFilesMessage { CoberturaFile = reportResult.CoberturaFile, ReportResult = reportResult.Report });
        }

        public void RunAndProcessReport(string[] coberturaFiles, List<ICoverageProject> coverageProjects, Action cleanUp = null)
            => RunAndProcessReport(new CoverageCollectionResult(coberturaFiles, coverageProjects), cleanUp);

        public void RunAndProcessReport(CoverageCollectionResult collection, Action cleanUp = null)
            => RunCancellableCoverageTask(
                async (vsShutdownLinkedCancellationToken) =>
                {
                    var reportResult = new ReportResult();

                    if (collection.HasCoverage)
                        reportResult = await RunAndProcessReportAsync(collection.CoberturaFiles.ToArray(), collection.CoverageProjects.ToList(), vsShutdownLinkedCancellationToken);

                    reportResult.Collection = collection;
                    return reportResult;
                },
                cleanUp);

        private async Task DoWorkAsync(
            Func<CancellationToken, Task<ReportResult>> reportResultProvider,
            ICancellationTokenSource source)
        {
            CancellationToken vsShutdownLinkedCancellationToken = source.Token;
            try
            {
                vsShutdownLinkedCancellationToken.ThrowIfCancellationRequested();
                ReportResult result = await reportResultProvider(vsShutdownLinkedCancellationToken);
                await LogCoverageStatusAsync("Done");
                vsShutdownLinkedCancellationToken.ThrowIfCancellationRequested();
                if (!IsCurrentRun(source))
                    return;

                // Listeners may synchronously switch to the UI thread, where a new run can start.
                // Never hold the lifecycle lock while notifying them.
                _eventAggregator.SendMessage(new CoverageEndedMessage());
                vsShutdownLinkedCancellationToken.ThrowIfCancellationRequested();
                if (result.Report == null || !IsCurrentRun(source))
                    return;

                _eventAggregator.SendMessage(new NewReportMessage(result.Report, result.CoverageProjects) { Collection = result.Collection });
                vsShutdownLinkedCancellationToken.ThrowIfCancellationRequested();
                RaiseReportFiles(result);
            }
            catch (OperationCanceledException)
            {
                if (!_disposeAwareTaskRunner.IsVsShutdown)
                {
                    await LogCoverageStatusAsync("Cancelled");
                    EndCurrentRun(source);
                }
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(
                    StatusMarkerProvider.Get("Error"),
                    ex.ToString());
                EndCurrentRun(source);
            }
        }

        private bool IsCurrentRun(ICancellationTokenSource source)
        {
            lock (_runLock)
            {
                return ReferenceEquals(_cancellationTokenSource, source);
            }
        }

        private void EndCurrentRun(ICancellationTokenSource source)
        {
            if (IsCurrentRun(source))
            {
                _eventAggregator.SendMessage(new CoverageEndedMessage());
            }
        }

        private void RunCancellableCoverageTask(
            Func<CancellationToken, Task<ReportResult>> reportResultProvider, Action cleanUp)
        {
            ICancellationTokenSource source = Reset();
            _disposeAwareTaskRunner.RunAsyncFunc(async () =>
            {
                await _reportGate.WaitAsync();
                try
                {
                    await _threadHelper.AwaitTaskSchedulerDefaultAsync();
                    await DoWorkAsync(reportResultProvider, source);
                }
                finally
                {
                    try
                    {
                        cleanUp?.Invoke();
                    }
                    finally
                    {
                        lock (_runLock)
                        {
                            if (ReferenceEquals(_cancellationTokenSource, source))
                            {
                                _cancellationTokenSource = null;
                            }
                        }

                        source.Dispose();
                        _ = _reportGate.Release();
                    }
                }
            });
        }
    }
}
