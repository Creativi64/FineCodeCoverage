using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine;
using FineCodeCoverage.Engine.Messages;
using FineCodeCoverage.Impl;
using FineCodeCoverage.Impl.TestContainerDiscovery;
using FineCodeCoverage.Output;
using Microsoft.CodeAnalysis;
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    [Export(typeof(ITUnitCoverage))]
    [Export(typeof(ICoverageCollectableFromTestExplorer))]
    internal class TUnitCoverage : ITUnitCoverage, ICoverageCollectableFromTestExplorer
    {
        private readonly ITUnitProjectsProvider tUnitProjectsProvider;
        private readonly IBuildHelper buildHelper;
        private readonly ITUnitCoverageProjectFactory tUnitCoverageProjectFactory;
        private readonly ITUnitCoverageRunner tUnitCoverageRunner;
        private readonly ICoverageToolOutputManager coverageToolOutputManager;
        private readonly IFCCEngine fccEngine;
        private readonly ITUnitSettingsProvider tUnitSettingsProvider;
        private readonly IDisposeAwareTaskRunner disposeAwareTaskRunner;
        private readonly IEventAggregator eventAggregator;
        private readonly ILogger logger;
        private int coverageRunNumber = 1;
        private ICancellationTokenSource cancellationTokenSource;
        private bool runnerReady;
        private bool projectsProviderReady;
        private bool externalBuildInProgress;

        public event EventHandler ReadyEvent;
        public event EventHandler<bool> CollectingChangedEvent;

        [ImportingConstructor]
        public TUnitCoverage(
            ITUnitProjectsProvider tUnitProjectsProvider,
            IBuildHelper buildHelper,
            ITUnitCoverageProjectFactory tUnitCoverageProjectFactory,
            ITUnitCoverageRunner tUnitCoverageRunner,
            ICoverageToolOutputManager coverageToolOutputManager,
            IFCCEngine fccEngine,
            ITUnitSettingsProvider tUnitSettingsProvider,
            IDisposeAwareTaskRunner disposeAwareTaskRunner,
            IEventAggregator eventAggregator,
            ILogger logger
        )
        {
            buildHelper.ExternalBuildEvent += this.BuildHelper_ExternalBuildEvent;
            tUnitProjectsProvider.ReadyEvent += this.TUnitProjectsProvider_ReadyEvent;
            tUnitCoverageRunner.ReadyEvent += this.TUnitRunner_ReadyEvent;
            this.projectsProviderReady = tUnitProjectsProvider.Ready;
            this.tUnitProjectsProvider = tUnitProjectsProvider;
            this.buildHelper = buildHelper;
            this.tUnitCoverageProjectFactory = tUnitCoverageProjectFactory;
            this.tUnitCoverageRunner = tUnitCoverageRunner;
            this.coverageToolOutputManager = coverageToolOutputManager;
            this.fccEngine = fccEngine;
            this.tUnitSettingsProvider = tUnitSettingsProvider;
            this.disposeAwareTaskRunner = disposeAwareTaskRunner;
            this.eventAggregator = eventAggregator;
            this.logger = logger;
        }

        private void BuildHelper_ExternalBuildEvent(object sender, BuildStartEndArgs e)
        {
            this.externalBuildInProgress = e.IsStart;
            this.OnReady();
        }

        private void TUnitRunner_ReadyEvent(object sender, EventArgs e)
        {
            this.runnerReady = true;
            this.OnReady();
        }

        private void TUnitProjectsProvider_ReadyEvent(object sender, EventArgs e)
        {
            this.projectsProviderReady = this.tUnitProjectsProvider.Ready;
            this.OnReady();
        }

        public bool Ready => this.runnerReady && this.projectsProviderReady && !this.externalBuildInProgress;
        private void OnReady() => ReadyEvent?.Invoke(this, EventArgs.Empty);

        protected void OnCollectingChanged(bool collecting) => CollectingChangedEvent?.Invoke(this, collecting);

        public void Cancel()
        {
            try
            {
                if (this.cancellationTokenSource?.IsCancellationRequested == false)
                {
                    this.cancellationTokenSource.Cancel();
                }
            }
            catch (ObjectDisposedException) { }
        }

        public void CollectCoverage()
        {
            this.Cancel();
            this.cancellationTokenSource = this.disposeAwareTaskRunner.CreateLinkedTokenSource();
            this.disposeAwareTaskRunner.RunAsyncFunc(this.CollectCoverageAsync);
        }

        private Task LogCoverageStartingAsync()
            => this.logger.LogAsync(StatusMarkerProvider.Get($"Coverage Starting - {this.coverageRunNumber++}"));

        private async Task<List<ITUnitCoverageProject>> GetEnabledTUnitProjectsAsync(CancellationToken cancellationToken)
        {
            List<ITUnitProject> tUnitProjects = await this.tUnitProjectsProvider.GetTUnitProjectsAsync(cancellationToken);
            ITUnitCoverageProject[] tUnitCoverageProjects = await Task.WhenAll(tUnitProjects.Select(tUnitProject => this.tUnitCoverageProjectFactory.CreateTUnitCoverageProjectAsync(tUnitProject, cancellationToken)));
            return tUnitCoverageProjects.Where(tp => tp.CoverageProject.Settings.Enabled).ToList();
        }

        private async Task CollectCoverageAsync()
        {
            CancellationToken cancellationToken = this.cancellationTokenSource.Token;
            await this.LogCoverageStartingAsync();

            this.OnCollectingChanged(true);//order important
            this.eventAggregator.SendMessage(new TestExecutionStartingMessage());
            this.eventAggregator.SendMessage(new CoverageStartingMessage());

            bool raiseCoverageEndedMessage = true;
            try
            {
                List<ITUnitCoverageProject> tUnitCoverageProjects = await this.GetEnabledTUnitProjectsAsync(cancellationToken);
                if (tUnitCoverageProjects.Any())
                {
                    bool success = await this.BuildAndCollectAsync(tUnitCoverageProjects, cancellationToken);
                    raiseCoverageEndedMessage = !success;
                }
                else
                {
                    await this.logger.LogAsync("No enabled Tunit test projects.");
                }
            }
            catch (OperationCanceledException)
            {
                await this.logger.LogAsync("Coverage collection cancelled");
            }
            catch (Exception exc)
            {
                await this.logger.LogAsync(exc.ToString());
            }

            if (raiseCoverageEndedMessage)
            {
                this.eventAggregator.SendMessage(new CoverageEndedMessage());
            }

            this.cancellationTokenSource.Dispose();
            this.cancellationTokenSource = null;
            this.OnCollectingChanged(false);
        }

        private async Task<bool> BuildAndCollectAsync(List<ITUnitCoverageProject> tUnitCoverageProjects, CancellationToken cancellationToken)
        {
            bool success = false;
            await this.logger.LogAsync("Starting build");
            bool buildSuccess = await this.buildHelper.BuildAsync(tUnitCoverageProjects.ConvertAll(tp => tp.VsHierarchy), cancellationToken);
            if (buildSuccess)
            {
                success = await this.CollectCoverageAsync(tUnitCoverageProjects, cancellationToken);
            }
            else
            {
                await this.logger.LogAsync("Unsuccessful build.  Not collecting coverage");
            }

            return success;
        }

        private async Task<bool> CollectCoverageAsync(List<ITUnitCoverageProject> tUnitCoverageProjects, CancellationToken cancellationToken)
        {
            await this.logger.LogAsync($"Collecting coverage for {tUnitCoverageProjects.Count} enabled TUnit test projects with coverage extension");

            List<Engine.Model.ICoverageProject> coverageProjects = tUnitCoverageProjects.ConvertAll(tUnitCoverageProject => tUnitCoverageProject.CoverageProject);
            cancellationToken.ThrowIfCancellationRequested();
            await this.coverageToolOutputManager.SetProjectCoverageOutputFolderAsync(coverageProjects);

            bool runAllProjects = true;
            var coberturaFiles = new List<string>();
            foreach (ITUnitCoverageProject tUnitCoverageProject in tUnitCoverageProjects)
            {
                TUnitSettings tUnitSettings = await this.tUnitSettingsProvider.ProvideAsync(tUnitCoverageProject, cancellationToken);
                bool success = await this.tUnitCoverageRunner.RunAsync(tUnitSettings, tUnitCoverageProject.HasCoverageExtension, false, cancellationToken);
                if (success)
                {
                    coberturaFiles.Add(tUnitSettings.OutputPath);
                }
                else
                {
                    runAllProjects = false;
                    break;
                }
            }

            if (runAllProjects)
            {
                this.fccEngine.RunAndProcessReport(coberturaFiles.ToArray(), coverageProjects);
            }
            else
            {
                await this.logger.LogAsync("Not collecting coverage due to unsuccessful test");
            }

            return runAllProjects;
        }

        async Task<bool> ICoverageCollectableFromTestExplorer.IsCollectableAsync()
        {
            List<ITUnitProject> tunitProjects = await this.tUnitProjectsProvider.GetTUnitProjectsAsync(CancellationToken.None);
            return !tunitProjects.Any();
        }
    }
}