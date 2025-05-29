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
        internal const string initializationFailedMessagePrefix = "Initialization failed.  Please check the following error which may be resolved by reopening visual studio which will start the initialization process again.";
        private ICancellationTokenSource cancellationTokenSource;

        private readonly ICoverageUtilManager coverageUtilManager;
        private readonly IReportGeneratorUtil reportGeneratorUtil;
        private readonly ILogger logger;

        private readonly ICoverageToolOutputManager coverageOutputManager;
        private readonly IEventAggregator eventAggregator;
        private readonly IDisposeAwareTaskRunner disposeAwareTaskRunner;

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
            this.eventAggregator = eventAggregator;
            this.disposeAwareTaskRunner = disposeAwareTaskRunner;
            this.coverageOutputManager = coverageOutputManager;
            this.coverageUtilManager = coverageUtilManager;
            this.reportGeneratorUtil = reportGeneratorUtil;
            this.logger = logger;
        }

        private Task LogCoverageStatusAsync(string reloadCoverageStatus)
        {
            return logger.LogAsync(StatusMarkerProvider.Get(reloadCoverageStatus));
        }

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

        private void Reset()
        {
            StopCoverage();

            cancellationTokenSource = disposeAwareTaskRunner.CreateLinkedTokenSource();
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

                    await coverageUtilManager.RunCoverageAsync(project, vsShutdownLinkedCancellationToken);

                    TimeSpan duration = DateTime.Now - start;
                    string durationMessage = $"Completed coverage for ({coverageProject.ProjectName}) : {duration}";
                    await logger.LogAsync(durationMessage);

                });

                if (coverageProject.HasFailed)
                {
                    string coverageStagePrefix = String.IsNullOrEmpty(coverageProject.FailureStage) ? "" : $"{coverageProject.FailureStage} ";
                    string failureMessage = $"{coverageProject.FailureStage}({coverageProject.ProjectName}) Failed.";
                    await logger.LogAsync(failureMessage, coverageProject.FailureDescription);
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
            string reportOutputFolder = coverageOutputManager.GetReportOutputFolder();
            vsShutdownLinkedCancellationToken.ThrowIfCancellationRequested();
            ReportGeneratorResult result = await reportGeneratorUtil.GenerateAsync(coverOutputFiles, reportOutputFolder, vsShutdownLinkedCancellationToken);

            vsShutdownLinkedCancellationToken.ThrowIfCancellationRequested();
            await logger.LogAsync("Processing cobertura");

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
                if (logs.Any())
                {
                    logs.Insert(0, "File synchronization :");
                    logs.Add($"File synchronization duration : {fileSynchronizationDetails.Duration}");
                    await logger.LogAsync(logs);
                }
            }
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
                    reportResult = await RunAndProcessReportAsync(coberturaFiles, coverageProjects, vsShutdownLinkedCancellationToken);
                }

                return reportResult;
            }, cleanUp);
        }

        private async Task DoWorkAsync(
            Func<CancellationToken, Task<ReportResult>> reportResultProvider,
            CancellationToken vsShutdownLinkedCancellationToken)
        {
            try
            {
                ReportResult result = await reportResultProvider(vsShutdownLinkedCancellationToken);
                await LogCoverageStatusAsync("Done");
                this.eventAggregator.SendMessage(new CoverageEndedMessage());
                this.eventAggregator.SendMessage(new NewReportMessage(result.Report, result.CoverageProjects));
                RaiseReportFiles(result);

            }
            catch (OperationCanceledException)
            {
                if (!disposeAwareTaskRunner.IsVsShutdown)
                {
                    await LogCoverageStatusAsync("Cancelled");
                    this.eventAggregator.SendMessage(new CoverageEndedMessage());
                }
            }
            catch (Exception ex)
            {
                await logger.LogAsync(
                    StatusMarkerProvider.Get("Error"),
                    ex.ToString()
                );
                this.eventAggregator.SendMessage(new CoverageEndedMessage());
            }
        }

        private void RunCancellableCoverageTask(
            Func<CancellationToken, Task<ReportResult>> reportResultProvider, Action cleanUp)
        {
            Reset();
            CancellationToken vsShutdownLinkedCancellationToken = cancellationTokenSource.Token;
            disposeAwareTaskRunner.RunAsyncFunc(async () =>
            {
                await TaskScheduler.Default;
                await DoWorkAsync(reportResultProvider, vsShutdownLinkedCancellationToken);
                cleanUp?.Invoke();
                cancellationTokenSource.Dispose();
            });
        }

        public void ReloadCoverage(Func<Task<List<ICoverageProject>>> coverageRequestCallback)
        {
            RunCancellableCoverageTask(async (vsShutdownLinkedCancellationToken) =>
            {
                ReportResult reportResult = new ReportResult();

                List<ICoverageProject> coverageProjects = await coverageRequestCallback();
                vsShutdownLinkedCancellationToken.ThrowIfCancellationRequested();

                await coverageOutputManager.SetProjectCoverageOutputFolderAsync(coverageProjects);

                string[] coverOutputFiles = await RunCoverageAsync(coverageProjects, vsShutdownLinkedCancellationToken);
                if (coverOutputFiles.Any())
                {
                    reportResult = await RunAndProcessReportAsync(coverOutputFiles, coverageProjects, vsShutdownLinkedCancellationToken);
                }

                return reportResult;
            }, null);
        }
    }

}