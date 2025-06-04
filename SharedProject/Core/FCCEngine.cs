using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine.Messages;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Engine.ReportGenerator;
using FineCodeCoverage.Output;
using Microsoft.VisualStudio.Threading;

namespace FineCodeCoverage.Engine
{
    [Export(typeof(IFCCEngine))]
    internal class FCCEngine : IFCCEngine
    {
        private class ReportResult
        {
            public IReportResult Report { get; set; }
            public string CoberturaFile { get; set; }
            public List<ICoverageProject> CoverageProjects { get; internal set; }
        }

        internal int InitializeWait { get; set; } = 5000;
        internal const string InitializationFailedMessagePrefix = "Initialization failed.  Please check the following error which may be resolved by reopening visual studio which will start the initialization process again.";
        private ICancellationTokenSource _cancellationTokenSource;

        private readonly ICoverageUtilManager _coverageUtilManager;
        private readonly IReportGeneratorUtil _reportGeneratorUtil;
        private readonly ILogger _logger;

        private readonly ICoverageToolOutputManager _coverageOutputManager;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDisposeAwareTaskRunner _disposeAwareTaskRunner;

        [ImportingConstructor]
        public FCCEngine(
            ICoverageUtilManager coverageUtilManager,
            IReportGeneratorUtil reportGeneratorUtil,
            ILogger logger,
            ICoverageToolOutputManager coverageOutputManager,
            IEventAggregator eventAggregator,
            IDisposeAwareTaskRunner disposeAwareTaskRunner
            )
        {
            this._eventAggregator = eventAggregator;
            this._disposeAwareTaskRunner = disposeAwareTaskRunner;
            this._coverageOutputManager = coverageOutputManager;
            this._coverageUtilManager = coverageUtilManager;
            this._reportGeneratorUtil = reportGeneratorUtil;
            this._logger = logger;
        }

        private Task LogCoverageStatusAsync(string reloadCoverageStatus)
            => this._logger.LogAsync(StatusMarkerProvider.Get(reloadCoverageStatus));

        public void StopCoverage()
        {
            if (this._cancellationTokenSource == null)
            {
                return;
            }

            try
            {
                this._cancellationTokenSource.Cancel();
            }
            catch (ObjectDisposedException) { }
        }

        private void Reset()
        {
            this.StopCoverage();

            this._cancellationTokenSource = this._disposeAwareTaskRunner.CreateLinkedTokenSource();
        }

        private async Task<string[]> RunCoverageAsync(List<ICoverageProject> coverageProjects, CancellationToken vsShutdownLinkedCancellationToken)
        {
            // process pipeline

            await this.PrepareCoverageProjectsAsync(coverageProjects, vsShutdownLinkedCancellationToken);

            foreach (ICoverageProject coverageProject in coverageProjects)
            {
                await coverageProject.StepAsync("Run Coverage Tool", async (project) =>
                {
                    DateTime start = DateTime.Now;

                    await this._coverageUtilManager.RunCoverageAsync(project, vsShutdownLinkedCancellationToken);

                    TimeSpan duration = DateTime.Now - start;
                    string durationMessage = $"Completed coverage for ({coverageProject.ProjectName}) : {duration}";
                    await this._logger.LogAsync(durationMessage);

                });

                if (coverageProject.HasFailed)
                {
                    string coverageStagePrefix = string.IsNullOrEmpty(coverageProject.FailureStage) ? "" : $"{coverageProject.FailureStage} ";
                    string failureMessage = $"{coverageProject.FailureStage}({coverageProject.ProjectName}) Failed.";
                    await this._logger.LogAsync(failureMessage, coverageProject.FailureDescription);
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
            string reportOutputFolder = this._coverageOutputManager.GetReportOutputFolder();
            vsShutdownLinkedCancellationToken.ThrowIfCancellationRequested();
            ReportGeneratorResult result = await this._reportGeneratorUtil.GenerateAsync(coverOutputFiles, reportOutputFolder, vsShutdownLinkedCancellationToken);

            vsShutdownLinkedCancellationToken.ThrowIfCancellationRequested();
            await this._logger.LogAsync("Processing cobertura");

            vsShutdownLinkedCancellationToken.ThrowIfCancellationRequested();

            return new ReportResult
            {
                CoberturaFile = result.UnifiedXmlFile,
                Report = result.ReportResult,
                CoverageProjects = coverageProjects
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
                    await this._logger.LogAsync(logs);
                }
            }
        }

        private void RaiseReportFiles(ReportResult reportResult)
        {
            if (reportResult.CoberturaFile == null)
            {
                return;
            }

            this._eventAggregator.SendMessage(new ReportFilesMessage { CoberturaFile = reportResult.CoberturaFile, ReportResult = reportResult.Report });
        }

        public void RunAndProcessReport(string[] coberturaFiles, List<ICoverageProject> coverageProjects, Action cleanUp = null)
            => this.RunCancellableCoverageTask(async (vsShutdownLinkedCancellationToken) =>
            {
                var reportResult = new ReportResult();

                if (coberturaFiles.Length != 0)
                {
                    reportResult = await this.RunAndProcessReportAsync(coberturaFiles, coverageProjects, vsShutdownLinkedCancellationToken);
                }

                return reportResult;
            }, cleanUp);

        private async Task DoWorkAsync(
            Func<CancellationToken, Task<ReportResult>> reportResultProvider,
            CancellationToken vsShutdownLinkedCancellationToken)
        {
            try
            {
                ReportResult result = await reportResultProvider(vsShutdownLinkedCancellationToken);
                await this.LogCoverageStatusAsync("Done");
                this._eventAggregator.SendMessage(new CoverageEndedMessage());
                this._eventAggregator.SendMessage(new NewReportMessage(result.Report, result.CoverageProjects));
                this.RaiseReportFiles(result);

            }
            catch (OperationCanceledException)
            {
                if (!this._disposeAwareTaskRunner.IsVsShutdown)
                {
                    await this.LogCoverageStatusAsync("Cancelled");
                    this._eventAggregator.SendMessage(new CoverageEndedMessage());
                }
            }
            catch (Exception ex)
            {
                await this._logger.LogAsync(
                    StatusMarkerProvider.Get("Error"),
                    ex.ToString()
                );
                this._eventAggregator.SendMessage(new CoverageEndedMessage());
            }
        }

        private void RunCancellableCoverageTask(
            Func<CancellationToken, Task<ReportResult>> reportResultProvider, Action cleanUp)
        {
            this.Reset();
            CancellationToken vsShutdownLinkedCancellationToken = this._cancellationTokenSource.Token;
            this._disposeAwareTaskRunner.RunAsyncFunc(async () =>
            {
                await TaskScheduler.Default;
                await this.DoWorkAsync(reportResultProvider, vsShutdownLinkedCancellationToken);
                cleanUp?.Invoke();
                this._cancellationTokenSource.Dispose();
            });
        }

        public void ReloadCoverage(Func<Task<List<ICoverageProject>>> coverageRequestCallback)
            => this.RunCancellableCoverageTask(async (vsShutdownLinkedCancellationToken) =>
            {
                var reportResult = new ReportResult();

                List<ICoverageProject> coverageProjects = await coverageRequestCallback();
                vsShutdownLinkedCancellationToken.ThrowIfCancellationRequested();

                await this._coverageOutputManager.SetProjectCoverageOutputFolderAsync(coverageProjects);

                string[] coverOutputFiles = await this.RunCoverageAsync(coverageProjects, vsShutdownLinkedCancellationToken);
                if (coverOutputFiles.Length != 0)
                {
                    reportResult = await this.RunAndProcessReportAsync(coverOutputFiles, coverageProjects, vsShutdownLinkedCancellationToken);
                }

                return reportResult;
            }, null);
    }
}
