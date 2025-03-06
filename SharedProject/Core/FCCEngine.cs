using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine.Cobertura;
using FineCodeCoverage.Engine.Messages;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Engine.MsTestPlatform;
using FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage;
using FineCodeCoverage.Engine.ReportGenerator;
using FineCodeCoverage.Options;
using FineCodeCoverage.Output;

namespace FineCodeCoverage.Engine
{
    internal sealed class NewCoverageLinesMessage
    {
        public IFileLineCoverage CoverageLines { get; set; }
    }

    internal class CoverageTaskState
    {
        public CancellationTokenSource CancellationTokenSource { get; set; }
        public Action CleanUp { get; set; }
    }

    internal class ReportResult
    {
        public IFileLineCoverage FileLineCoverage { get; set; }
        public IReportResult Report { get; set; }
        public string CoberturaFile { get; set; }
        public List<ICoverageProject> CoverageProjects { get; internal set; }
    }

    class ReportFilesMessage
    {
        public IReportResult ReportResult { get; set; }
        public string CoberturaFile { get; set; }
    }

    [Export(typeof(IFCCEngine))]
    internal class FCCEngine : IFCCEngine,IDisposable
    {
        internal int InitializeWait { get; set; } = 5000;
        internal const string initializationFailedMessagePrefix = "Initialization failed.  Please check the following error which may be resolved by reopening visual studio which will start the initialization process again.";
        private CancellationTokenSource cancellationTokenSource;

        public string AppDataFolderPath { get; private set; }
        private bool IsVsShutdown => disposeAwareTaskRunner.DisposalToken.IsCancellationRequested;

        private readonly ICoverageUtilManager coverageUtilManager;
        private readonly ICoberturaUtil coberturaUtil;
        private readonly IMsCodeCoverageRunSettingsService msCodeCoverageRunSettingsService;
        private readonly IMsTestPlatformUtil msTestPlatformUtil;
        private readonly IReportGeneratorUtil reportGeneratorUtil;
        private readonly ILogger logger;
        private readonly IAppDataFolder appDataFolder;

        private readonly ICoverageToolOutputManager coverageOutputManager;
        internal Task reloadCoverageTask;
#pragma warning disable IDE0052 // Remove unread private members
        private readonly ISolutionEvents solutionEvents; // keep alive
#pragma warning restore IDE0052 // Remove unread private members
        private readonly IEventAggregator eventAggregator;
        private readonly IDisposeAwareTaskRunner disposeAwareTaskRunner;
        private bool disposed = false;

        [ImportingConstructor]
        public FCCEngine(
            ICoverageUtilManager coverageUtilManager,
            ICoberturaUtil coberturaUtil,
            IMsTestPlatformUtil msTestPlatformUtil,
            IReportGeneratorUtil reportGeneratorUtil,
            ILogger logger,
            IAppDataFolder appDataFolder,
            ICoverageToolOutputManager coverageOutputManager,
            IMsCodeCoverageRunSettingsService msCodeCoverageRunSettingsService,
            ISolutionEvents solutionEvents,
            IAppOptionsProvider appOptionsProvider,
            IEventAggregator eventAggregator,
            IDisposeAwareTaskRunner disposeAwareTaskRunner
            )
        {
            this.solutionEvents = solutionEvents;
            this.eventAggregator = eventAggregator;
            this.disposeAwareTaskRunner = disposeAwareTaskRunner;
            solutionEvents.AfterClosing += (s,args) => ClearUI();
            appOptionsProvider.OptionsChanged += (appOptions) =>
            {
                if (!appOptions.Enabled)
                {
                    ClearUI();
                }
            };
            this.coverageOutputManager = coverageOutputManager;
            this.coverageUtilManager = coverageUtilManager;
            this.coberturaUtil = coberturaUtil;
            this.msTestPlatformUtil = msTestPlatformUtil;
            this.reportGeneratorUtil = reportGeneratorUtil;
            this.logger = logger;
            this.appDataFolder = appDataFolder;
            this.msCodeCoverageRunSettingsService = msCodeCoverageRunSettingsService;
        }

        private void LogCoverageStatus(string reloadCoverageStatus)
        {
            logger.Log(StatusMarkerProvider.Get(reloadCoverageStatus));
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            await appDataFolder.InitializeAsync(cancellationToken);
            AppDataFolderPath = appDataFolder.DirectoryPath;

            msTestPlatformUtil.Initialize(AppDataFolderPath, cancellationToken);
            coverageUtilManager.Initialize(AppDataFolderPath, cancellationToken);
            msCodeCoverageRunSettingsService.Initialize(AppDataFolderPath, this,cancellationToken);
        }

        public void ClearUI()
        {
            ClearCoverageLines();
            this.RaiseNewReport(null);
        }

        private void RaiseNewReport(IReportResult reportResult) => this.eventAggregator.SendMessage(new NewReportMessage { Report = reportResult});

        public void StopCoverage()
        {
            if (cancellationTokenSource != null)
            {
                try
                {
                    cancellationTokenSource.Cancel();
                }
                catch (ObjectDisposedException) { }
            }
        }

        private CancellationTokenSource Reset()
        {
            ClearCoverageLines();

            StopCoverage();

            cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(disposeAwareTaskRunner.DisposalToken);

            return cancellationTokenSource;
        }

        private async Task<string[]> RunCoverageAsync(List<ICoverageProject> coverageProjects,CancellationToken vsShutdownLinkedCancellationToken)
        {
            // process pipeline

            await PrepareCoverageProjectsAsync(coverageProjects, vsShutdownLinkedCancellationToken);

            foreach (var coverageProject in coverageProjects)
            {
                await coverageProject.StepAsync("Run Coverage Tool", async (project) =>
                {
                    var start = DateTime.Now;

                    await coverageUtilManager.RunCoverageAsync(project, vsShutdownLinkedCancellationToken);

                    var duration = DateTime.Now - start;
                    var durationMessage = $"Completed coverage for ({coverageProject.ProjectName}) : {duration}";
                    await logger.LogAsync(durationMessage);

                });

                if (coverageProject.HasFailed)
                {
                    var coverageStagePrefix = String.IsNullOrEmpty(coverageProject.FailureStage) ? "" : $"{coverageProject.FailureStage} ";
                    var failureMessage = $"{coverageProject.FailureStage}({coverageProject.ProjectName}) Failed.";
                    await logger.LogAsync(failureMessage, coverageProject.FailureDescription);
                }

            }

            var passedProjects = coverageProjects.Where(p => !p.HasFailed);

            return passedProjects
                    .Select(x => x.CoverageOutputFile)
                    .ToArray();

        }


        private void ClearCoverageLines()
        {
            RaiseCoverageLines(null);
        }

        private void RaiseCoverageLines(IFileLineCoverage coverageLines)
        {
            eventAggregator.SendMessage(new NewCoverageLinesMessage { CoverageLines = coverageLines});
        }

        private void UpdateUI(IFileLineCoverage coverageLines, IReportResult reportResult)
        {
            RaiseCoverageLines(coverageLines);
            RaiseNewReport(reportResult);
        }

        private async Task<ReportResult> RunAndProcessReportAsync(string[] coverOutputFiles, CancellationToken vsShutdownLinkedCancellationToken)
        {
            var reportOutputFolder = coverageOutputManager.GetReportOutputFolder();
            vsShutdownLinkedCancellationToken.ThrowIfCancellationRequested();
            var result =  await reportGeneratorUtil.GenerateAsync(coverOutputFiles,reportOutputFolder,vsShutdownLinkedCancellationToken);

            vsShutdownLinkedCancellationToken.ThrowIfCancellationRequested();
            await logger.LogAsync("Processing cobertura");
            var coverageLines = coberturaUtil.ProcessCoberturaXml(result.UnifiedXmlFile);

            vsShutdownLinkedCancellationToken.ThrowIfCancellationRequested();

            return new ReportResult
            {
                FileLineCoverage = coverageLines,
                CoberturaFile = result.UnifiedXmlFile,
                Report = result.ReportResult
            };
        }

        private async Task PrepareCoverageProjectsAsync(List<ICoverageProject> coverageProjects, CancellationToken cancellationToken)
        {
            foreach (var project in coverageProjects)
            {
                if (string.IsNullOrWhiteSpace(project.ProjectFile))
                {
                    project.FailureDescription = $"Unsupported project type for DLL '{project.TestDllFile}'";
                    continue;
                }

                if (!project.Settings.Enabled)
                {
                    project.FailureDescription = "Disabled";
                    continue;
                }

                var fileSynchronizationDetails = await project.PrepareForCoverageAsync(cancellationToken);
                var logs = fileSynchronizationDetails.Logs;
                if (logs.Any())
                {
                    logs.Insert(0, "File synchronization :");
                    logs.Add($"File synchronization duration : {fileSynchronizationDetails.Duration}");
                    await logger.LogAsync(logs);
                }
            }
        }

        private void CoverageTaskCompletion(Task<ReportResult> t, object state)
        {
            var displayCoverageResultState = (CoverageTaskState)state;
            if (!IsVsShutdown)
            {
                switch (t.Status)
                {
                    case TaskStatus.Canceled:
                        LogCoverageStatus("Cancelled");
                        this.eventAggregator.SendMessage(new CoverageEndedMessage(null));
                        break;
                    case TaskStatus.Faulted:
                        var innerException = t.Exception.InnerExceptions[0];
                        logger.Log(
                            StatusMarkerProvider.Get("Error"),
                            innerException.ToString()
                        );
                        this.eventAggregator.SendMessage(new CoverageEndedMessage(null));
                        break;
                    case TaskStatus.RanToCompletion:
                        LogCoverageStatus("Done");
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
                        this.eventAggregator.SendMessage(new CoverageEndedMessage(t.Result.CoverageProjects));
                        UpdateUI(t.Result.FileLineCoverage, t.Result.Report);
                        RaiseReportFiles(t.Result);
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
                        break;
                }

            }
            displayCoverageResultState.CleanUp?.Invoke();
            displayCoverageResultState.CancellationTokenSource.Dispose();
        }

        private void RaiseReportFiles(ReportResult reportResult)
        {
            if (reportResult.CoberturaFile != null)
            {
                this.eventAggregator.SendMessage(new ReportFilesMessage { CoberturaFile = reportResult.CoberturaFile, ReportResult = reportResult.Report });
            }
        }

        public void RunAndProcessReport(string[] coberturaFiles, List<ICoverageProject> coverageProjects, Action cleanUp = null)
        {
            RunCancellableCoverageTask(async (vsShutdownLinkedCancellationToken) =>
            {
                ReportResult reportResult = new ReportResult();

                if (coberturaFiles.Any())
                {
                    reportResult = await RunAndProcessReportAsync(coberturaFiles, vsShutdownLinkedCancellationToken);
                }
                reportResult.CoverageProjects = coverageProjects;
                return reportResult;
            }, cleanUp);
        }

        private void RunCancellableCoverageTask(
            Func<CancellationToken, Task<ReportResult>> taskCreator, Action cleanUp)
        {
            var vsLinkedCancellationTokenSource = Reset();
            var vsShutdownLinkedCancellationToken = vsLinkedCancellationTokenSource.Token;
            disposeAwareTaskRunner.RunAsyncFunc(() =>
            {
                reloadCoverageTask = Task.Run(async () =>
                {
                    var result = await taskCreator(vsShutdownLinkedCancellationToken);
                    return result;

                }, vsShutdownLinkedCancellationToken)
                .ContinueWith(CoverageTaskCompletion, new CoverageTaskState { CancellationTokenSource = vsLinkedCancellationTokenSource, CleanUp = cleanUp}, System.Threading.Tasks.TaskScheduler.Default);
                return reloadCoverageTask;
            });
        }

        public void ReloadCoverage(Func<Task<List<ICoverageProject>>> coverageRequestCallback)
        {
            RunCancellableCoverageTask(async (vsShutdownLinkedCancellationToken) =>
            {
                ReportResult reportResult = new ReportResult();

                var coverageProjects = await coverageRequestCallback();
                vsShutdownLinkedCancellationToken.ThrowIfCancellationRequested();

                await coverageOutputManager.SetProjectCoverageOutputFolderAsync(coverageProjects);

                var coverOutputFiles = await RunCoverageAsync(coverageProjects, vsShutdownLinkedCancellationToken);
                if (coverOutputFiles.Any())
                {
                   reportResult = await RunAndProcessReportAsync(coverOutputFiles, vsShutdownLinkedCancellationToken);
                }

                return reportResult;
            },null);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing && cancellationTokenSource != null)
                {
                    cancellationTokenSource.Dispose();
                }

                disposed = true;
            }
        }
    }

}