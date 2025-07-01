using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Collection.CoverageProjectManagement;
using FineCodeCoverage.Collection.CoverageToolOutput;
using FineCodeCoverage.Collection.CoverletOpenCover;
using FineCodeCoverage.Collection.Messages;
using FineCodeCoverage.Collection.ReportGeneration;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Core.Utilities.VsThreading;
using FineCodeCoverage.Output;

namespace FineCodeCoverage.Engine
{
    [Export(typeof(IFCCEngine))]
    internal sealed class FCCEngine : IFCCEngine
    {
        private sealed class ReportResult
        {
            public IReportResult Report { get; set; }

            public string CoberturaFile { get; set; }

            public List<ICoverageProject> CoverageProjects { get; internal set; }
        }

        internal int InitializeWait { get; set; } = 5000;

        internal const string InitializationFailedMessagePrefix = "Initialization failed.  Please check the following error which may be resolved by reopening visual studio which will start the initialization process again.";

        private readonly ICoverageUtilManager _coverageUtilManager;
        private readonly IReportGeneratorUtil _reportGeneratorUtil;
        private readonly ILogger _logger;
        private readonly ICoverageToolOutputManager _coverageOutputManager;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDisposeAwareTaskRunner _disposeAwareTaskRunner;
        private readonly IThreadHelper _threadHelper;
        private ICancellationTokenSource _cancellationTokenSource;

        [ImportingConstructor]
        public FCCEngine(
            ICoverageUtilManager coverageUtilManager,
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
            _coverageUtilManager = coverageUtilManager;
            _reportGeneratorUtil = reportGeneratorUtil;
            _logger = logger;
        }

        private Task LogCoverageStatusAsync(string reloadCoverageStatus)
            => _logger.LogAsync(StatusMarkerProvider.Get(reloadCoverageStatus));

        public void StopCoverage()
        {
            if (_cancellationTokenSource == null)
            {
                return;
            }

            try
            {
                _cancellationTokenSource.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private void Reset()
        {
            StopCoverage();

            _cancellationTokenSource = _disposeAwareTaskRunner.CreateLinkedTokenSource();
        }

        private async Task<string[]> RunCoverageAsync(List<ICoverageProject> coverageProjects, CancellationToken vsShutdownLinkedCancellationToken)
        {
            // process pipeline
            await PrepareCoverageProjectsAsync(coverageProjects, vsShutdownLinkedCancellationToken);

            foreach (ICoverageProject coverageProject in coverageProjects)
            {
                await coverageProject.StepAsync("Run Coverage Tool", async (project) =>
                {
                    DateTime start = DateTime.Now;

                    await _coverageUtilManager.RunCoverageAsync(project, vsShutdownLinkedCancellationToken);

                    TimeSpan duration = DateTime.Now - start;
                    string durationMessage = $"Completed coverage for ({coverageProject.ProjectName}) : {duration}";
                    await _logger.LogAsync(durationMessage);
                });

                if (coverageProject.HasFailed)
                {
                    string coverageStagePrefix = string.IsNullOrEmpty(coverageProject.FailureStage) ? string.Empty : $"{coverageProject.FailureStage} ";
                    string failureMessage = $"{coverageProject.FailureStage}({coverageProject.ProjectName}) Failed.";
                    await _logger.LogAsync(failureMessage, coverageProject.FailureDescription);
                }
            }

            IEnumerable<ICoverageProject> passedProjects = coverageProjects.Where(p => !p.HasFailed);

            return passedProjects
                    .Select(x => x.CoverageOutputFile)
                    .ToArray();
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

        private async Task PrepareCoverageProjectsAsync(List<ICoverageProject> coverageProjects, CancellationToken cancellationToken)
        {
            foreach (ICoverageProject project in coverageProjects)
            {
                if (string.IsNullOrWhiteSpace(project.ProjectFilePath))
                {
                    project.FailureDescription = $"Unsupported project type for DLL '{project.TestDllFile}'";
                    continue;
                }

                if (!project.Settings.Enabled)
                {
                    project.FailureDescription = "Disabled";
                    continue;
                }

                CoverageProjectFileSynchronizationDetails fileSynchronizationDetails = await project.PrepareForCoverageAsync(cancellationToken);
                List<string> logs = fileSynchronizationDetails.Logs;
                if (logs.Count != 0)
                {
                    logs.Insert(0, "File synchronization :");
                    logs.Add($"File synchronization duration : {fileSynchronizationDetails.Duration}");
                    await _logger.LogAsync(logs);
                }
            }
        }

        private void RaiseReportFiles(ReportResult reportResult)
        {
            if (reportResult.CoberturaFile == null)
            {
                return;
            }

            _eventAggregator.SendMessage(new ReportFilesMessage { CoberturaFile = reportResult.CoberturaFile, ReportResult = reportResult.Report });
        }

        public void RunAndProcessReport(string[] coberturaFiles, List<ICoverageProject> coverageProjects, Action cleanUp = null)
            => RunCancellableCoverageTask(
                async (vsShutdownLinkedCancellationToken) =>
                {
                    var reportResult = new ReportResult();

                    if (coberturaFiles.Length != 0)
                    {
                        reportResult = await RunAndProcessReportAsync(coberturaFiles, coverageProjects, vsShutdownLinkedCancellationToken);
                    }

                    return reportResult;
                },
                cleanUp);

        private async Task DoWorkAsync(
            Func<CancellationToken, Task<ReportResult>> reportResultProvider,
            CancellationToken vsShutdownLinkedCancellationToken)
        {
            try
            {
                ReportResult result = await reportResultProvider(vsShutdownLinkedCancellationToken);
                await LogCoverageStatusAsync("Done");
                _eventAggregator.SendMessage(new CoverageEndedMessage());
                _eventAggregator.SendMessage(new NewReportMessage(result.Report, result.CoverageProjects));
                RaiseReportFiles(result);
            }
            catch (OperationCanceledException)
            {
                if (!_disposeAwareTaskRunner.IsVsShutdown)
                {
                    await LogCoverageStatusAsync("Cancelled");
                    _eventAggregator.SendMessage(new CoverageEndedMessage());
                }
            }
            catch (Exception ex)
            {
                await _logger.LogAsync(
                    StatusMarkerProvider.Get("Error"),
                    ex.ToString());
                _eventAggregator.SendMessage(new CoverageEndedMessage());
            }
        }

        private void RunCancellableCoverageTask(
            Func<CancellationToken, Task<ReportResult>> reportResultProvider, Action cleanUp)
        {
            Reset();
            CancellationToken vsShutdownLinkedCancellationToken = _cancellationTokenSource.Token;
            _disposeAwareTaskRunner.RunAsyncFunc(async () =>
            {
                await _threadHelper.AwaitTaskSchedulerDefaultAsync();
                await DoWorkAsync(reportResultProvider, vsShutdownLinkedCancellationToken);
                cleanUp?.Invoke();
                _cancellationTokenSource.Dispose();
            });
        }

        public void ReloadCoverage(Func<Task<List<ICoverageProject>>> coverageRequestCallback)
            => RunCancellableCoverageTask(
                async (vsShutdownLinkedCancellationToken) =>
                {
                    var reportResult = new ReportResult();

                    List<ICoverageProject> coverageProjects = await coverageRequestCallback();
                    vsShutdownLinkedCancellationToken.ThrowIfCancellationRequested();

                    await _coverageOutputManager.SetProjectCoverageOutputFolderAsync(coverageProjects);

                    string[] coverOutputFiles = await RunCoverageAsync(coverageProjects, vsShutdownLinkedCancellationToken);
                    if (coverOutputFiles.Length != 0)
                    {
                        reportResult = await RunAndProcessReportAsync(coverOutputFiles, coverageProjects, vsShutdownLinkedCancellationToken);
                    }

                    return reportResult;
                },
                null);
    }
}
