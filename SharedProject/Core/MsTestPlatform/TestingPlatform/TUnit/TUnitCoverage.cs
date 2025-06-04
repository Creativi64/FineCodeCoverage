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
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    [Export(typeof(ITUnitCoverage))]
    [Export(typeof(ICoverageCollectableFromTestExplorer))]
    internal class TUnitCoverage : ITUnitCoverage, ICoverageCollectableFromTestExplorer
    {
        private readonly ITUnitProjectsProvider _tUnitProjectsProvider;
        private readonly IBuildHelper _buildHelper;
        private readonly ITUnitCoverageProjectFactory _tUnitCoverageProjectFactory;
        private readonly ITUnitCoverageRunner _tUnitCoverageRunner;
        private readonly ICoverageToolOutputManager _coverageToolOutputManager;
        private readonly IFCCEngine _fccEngine;
        private readonly ITUnitSettingsProvider _tUnitSettingsProvider;
        private readonly IDisposeAwareTaskRunner _disposeAwareTaskRunner;
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger _logger;
        private int _coverageRunNumber = 1;
        private ICancellationTokenSource _cancellationTokenSource;
        private bool _runnerReady;
        private bool _projectsProviderReady;
        private bool _externalBuildInProgress;

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
            this._projectsProviderReady = tUnitProjectsProvider.Ready;
            this._tUnitProjectsProvider = tUnitProjectsProvider;
            this._buildHelper = buildHelper;
            this._tUnitCoverageProjectFactory = tUnitCoverageProjectFactory;
            this._tUnitCoverageRunner = tUnitCoverageRunner;
            this._coverageToolOutputManager = coverageToolOutputManager;
            this._fccEngine = fccEngine;
            this._tUnitSettingsProvider = tUnitSettingsProvider;
            this._disposeAwareTaskRunner = disposeAwareTaskRunner;
            this._eventAggregator = eventAggregator;
            this._logger = logger;
        }

        private void BuildHelper_ExternalBuildEvent(object sender, BuildStartEndArgs e)
        {
            this._externalBuildInProgress = e.IsStart;
            this.OnReady();
        }

        private void TUnitRunner_ReadyEvent(object sender, EventArgs e)
        {
            this._runnerReady = true;
            this.OnReady();
        }

        private void TUnitProjectsProvider_ReadyEvent(object sender, EventArgs e)
        {
            this._projectsProviderReady = this._tUnitProjectsProvider.Ready;
            this.OnReady();
        }

        public bool Ready => this._runnerReady && this._projectsProviderReady && !this._externalBuildInProgress;
        private void OnReady() => ReadyEvent?.Invoke(this, EventArgs.Empty);

        protected void OnCollectingChanged(bool collecting) => CollectingChangedEvent?.Invoke(this, collecting);

        public void Cancel()
        {
            try
            {
                if (this._cancellationTokenSource?.IsCancellationRequested == false)
                {
                    this._cancellationTokenSource.Cancel();
                }
            }
            catch (ObjectDisposedException) { }
        }

        public void CollectCoverage()
        {
            this.Cancel();
            this._cancellationTokenSource = this._disposeAwareTaskRunner.CreateLinkedTokenSource();
            this._disposeAwareTaskRunner.RunAsyncFunc(this.CollectCoverageAsync);
        }

        private Task LogCoverageStartingAsync()
            => this._logger.LogAsync(StatusMarkerProvider.Get($"Coverage Starting - {this._coverageRunNumber++}"));

        private async Task<List<ITUnitCoverageProject>> GetEnabledTUnitProjectsAsync(CancellationToken cancellationToken)
        {
            List<ITUnitProject> tUnitProjects = await this._tUnitProjectsProvider.GetTUnitProjectsAsync(cancellationToken);
            ITUnitCoverageProject[] tUnitCoverageProjects = await Task.WhenAll(tUnitProjects.Select(tUnitProject => this._tUnitCoverageProjectFactory.CreateTUnitCoverageProjectAsync(tUnitProject, cancellationToken)));
            return tUnitCoverageProjects.Where(tp => tp.CoverageProject.Settings.Enabled).ToList();
        }

        private async Task CollectCoverageAsync()
        {
            CancellationToken cancellationToken = this._cancellationTokenSource.Token;
            await this.LogCoverageStartingAsync();

            this.OnCollectingChanged(true);//order important
            this._eventAggregator.SendMessage(new TestExecutionStartingMessage());
            this._eventAggregator.SendMessage(new CoverageStartingMessage());

            bool raiseCoverageEndedMessage = true;
            try
            {
                List<ITUnitCoverageProject> tUnitCoverageProjects = await this.GetEnabledTUnitProjectsAsync(cancellationToken);
                if (tUnitCoverageProjects.Count != 0)
                {
                    bool success = await this.BuildAndCollectAsync(tUnitCoverageProjects, cancellationToken);
                    raiseCoverageEndedMessage = !success;
                }
                else
                {
                    await this._logger.LogAsync("No enabled Tunit test projects.");
                }
            }
            catch (OperationCanceledException)
            {
                await this._logger.LogAsync("Coverage collection cancelled");
            }
            catch (Exception exc)
            {
                await this._logger.LogAsync(exc.ToString());
            }

            if (raiseCoverageEndedMessage)
            {
                this._eventAggregator.SendMessage(new CoverageEndedMessage());
            }

            this._cancellationTokenSource.Dispose();
            this._cancellationTokenSource = null;
            this.OnCollectingChanged(false);
        }

        private async Task<bool> BuildAndCollectAsync(List<ITUnitCoverageProject> tUnitCoverageProjects, CancellationToken cancellationToken)
        {
            bool success = false;
            await this._logger.LogAsync("Starting build");
            bool buildSuccess = await this._buildHelper.BuildAsync(tUnitCoverageProjects.ConvertAll(tp => tp.VsHierarchy), cancellationToken);
            if (buildSuccess)
            {
                success = await this.CollectCoverageAsync(tUnitCoverageProjects, cancellationToken);
            }
            else
            {
                await this._logger.LogAsync("Unsuccessful build.  Not collecting coverage");
            }

            return success;
        }

        private async Task<bool> CollectCoverageAsync(List<ITUnitCoverageProject> tUnitCoverageProjects, CancellationToken cancellationToken)
        {
            await this._logger.LogAsync($"Collecting coverage for {tUnitCoverageProjects.Count} enabled TUnit test projects with coverage extension");

            List<Engine.Model.ICoverageProject> coverageProjects = tUnitCoverageProjects.ConvertAll(tUnitCoverageProject => tUnitCoverageProject.CoverageProject);
            cancellationToken.ThrowIfCancellationRequested();
            await this._coverageToolOutputManager.SetProjectCoverageOutputFolderAsync(coverageProjects);

            bool runAllProjects = true;
            var coberturaFiles = new List<string>();
            foreach (ITUnitCoverageProject tUnitCoverageProject in tUnitCoverageProjects)
            {
                TUnitSettings tUnitSettings = await this._tUnitSettingsProvider.ProvideAsync(tUnitCoverageProject, cancellationToken);
                bool success = await this._tUnitCoverageRunner.RunAsync(tUnitSettings, tUnitCoverageProject.HasCoverageExtension, false, cancellationToken);
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
                this._fccEngine.RunAndProcessReport(coberturaFiles.ToArray(), coverageProjects);
            }
            else
            {
                await this._logger.LogAsync("Not collecting coverage due to unsuccessful test");
            }

            return runAllProjects;
        }

        async Task<bool> ICoverageCollectableFromTestExplorer.IsCollectableAsync()
        {
            List<ITUnitProject> tunitProjects = await this._tUnitProjectsProvider.GetTUnitProjectsAsync(CancellationToken.None);
            return tunitProjects.Count == 0;
        }
    }
}
