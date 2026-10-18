using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Collection.CoverageProjectManagement;
using FineCodeCoverage.Collection.CoverageToolOutput;
using FineCodeCoverage.Collection.Engine;
using FineCodeCoverage.Collection.Messages;
using FineCodeCoverage.Collection.TestingPlatform.TUnit;
using FineCodeCoverage.Utilities.Events;
using FineCodeCoverage.Utilities.Logging;
using FineCodeCoverage.Utilities.Wrappers;
using FineCodeCoverage.Vs.Events;
using FineCodeCoverage.VSAbstractions.OutputWindow;
using FineCodeCoverage.VSAbstractions.Threading;
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Collection.Runners
{
    [Export(typeof(ITUnitCoverage))]
    [SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "The gate lives with the MEF singleton and never allocates AvailableWaitHandle. Disposing it during shutdown would race asynchronous cleanup.")]
    internal sealed class TUnitCoverage : ITUnitCoverage
    {
        private readonly ITUnitProjectsProvider _tUnitProjectsProvider;
        private readonly IBuildHelper _buildHelper;
        private readonly ITUnitCoverageProjectFactory _tUnitCoverageProjectFactory;
        private readonly ITUnitCoverageRunner _tUnitCoverageRunner;
        private readonly ICoverageToolOutputManager _coverageToolOutputManager;
        private readonly IFCCEngine _fccEngine;
        private readonly ITUnitSettingsProvider _tUnitSettingsProvider;
        private readonly ITestConfigInjector _testConfigInjector;
        private readonly IFileUtil _fileUtil;
        private readonly IDisposeAwareTaskRunner _disposeAwareTaskRunner;
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger _logger;
        private readonly object _runLock = new object();
        private readonly SemaphoreSlim _executionGate = new SemaphoreSlim(1, 1);
        private int _coverageRunNumber = 1;
        private CoverageRun _activeRun;
        private bool _runnerReady;
        private bool _projectsProviderReady;
        private bool _externalBuildInProgress;

        private sealed class CoverageRun
        {
            public CoverageRun(ICancellationTokenSource source)
            {
                Source = source;
                Token = source.Token;
            }

            public ICancellationTokenSource Source { get; }

            public CancellationToken Token { get; }

            public List<PreparedProject> Projects { get; } = new List<PreparedProject>();

            public bool Busy { get; set; } = true;

            public bool Prepared { get; set; }

            public bool Finished { get; set; }
        }

        /// <summary>
        /// A project that took part in the Test Explorer run, and how its coverage is being collected.
        /// </summary>
        private sealed class PreparedProject
        {
            public PreparedProject(
                ITUnitCoverageProject tUnitCoverageProject,
                TestConfigInjectionResult injection)
            {
                TUnitCoverageProject = tUnitCoverageProject;
                Injection = injection;
            }

            public ITUnitCoverageProject TUnitCoverageProject { get; }

            /// <summary>
            /// Null when coverage could not be injected. Automatic collection must not re-run the host
            /// because the Test Explorer's selected-test filter is not available here.
            /// </summary>
            public TestConfigInjectionResult Injection { get; }
        }

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
            ITestConfigInjector testConfigInjector,
            IFileUtil fileUtil,
            IDisposeAwareTaskRunner disposeAwareTaskRunner,
            IEventAggregator eventAggregator,
            IVsShutdown shutdown,
            ILogger logger)
        {
            shutdown.Shutdown += (_, __) => Cancel();
            buildHelper.ExternalBuildEvent += BuildHelper_ExternalBuildEvent;
            tUnitProjectsProvider.ReadyEvent += TUnitProjectsProvider_ReadyEvent;
            tUnitCoverageRunner.ReadyEvent += TUnitRunner_ReadyEvent;
            _projectsProviderReady = tUnitProjectsProvider.Ready;
            _tUnitProjectsProvider = tUnitProjectsProvider;
            _buildHelper = buildHelper;
            _tUnitCoverageProjectFactory = tUnitCoverageProjectFactory;
            _tUnitCoverageRunner = tUnitCoverageRunner;
            _coverageToolOutputManager = coverageToolOutputManager;
            _fccEngine = fccEngine;
            _tUnitSettingsProvider = tUnitSettingsProvider;
            _testConfigInjector = testConfigInjector;
            _fileUtil = fileUtil;
            _disposeAwareTaskRunner = disposeAwareTaskRunner;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        private void BuildHelper_ExternalBuildEvent(object sender, BuildStartEndArgs e)
        {
            _externalBuildInProgress = e.IsStart;
            OnReady();
        }

        private void TUnitRunner_ReadyEvent(object sender, EventArgs e)
        {
            _runnerReady = true;
            OnReady();
        }

        private void TUnitProjectsProvider_ReadyEvent(object sender, EventArgs e)
        {
            _projectsProviderReady = _tUnitProjectsProvider.Ready;
            OnReady();
        }

        public bool Ready => _runnerReady && _projectsProviderReady && !_externalBuildInProgress;

        private void OnReady() => ReadyEvent?.Invoke(this, EventArgs.Empty);

        private void OnCollectingChanged(bool collecting) => CollectingChangedEvent?.Invoke(this, collecting);

        public void Cancel()
        {
            bool notify;
            lock (_runLock)
            {
                notify = CancelCurrentRun();
            }

            if (!notify)
            {
                return;
            }

            OnCollectingChanged(false);
        }

        // Caller holds _runLock. UI notifications are sent only after releasing it.
        private bool CancelCurrentRun()
        {
            CoverageRun run = _activeRun;
            if (run == null)
            {
                return false;
            }

            run.Source.Cancel();
            return !run.Busy && FinishRunCore(run);
        }

        private CoverageRun BeginRun()
        {
            lock (_runLock)
            {
                _ = CancelCurrentRun();
                _activeRun = new CoverageRun(_disposeAwareTaskRunner.CreateLinkedTokenSource());
                return _activeRun;
            }
        }

        public void CollectCoverage()
        {
            CoverageRun run = BeginRun();
            _disposeAwareTaskRunner.RunAsyncFunc(() => CollectCoverageAsync(run));
        }

        private Task LogCoverageStartingAsync()
            => _logger.LogAsync(StatusMarkerProvider.Get($"Coverage Starting - {_coverageRunNumber++}"));

        private async Task<List<ITUnitCoverageProject>> GetEnabledTUnitProjectsAsync(CancellationToken cancellationToken)
        {
            List<ITUnitProject> tUnitProjects = await _tUnitProjectsProvider.GetTUnitProjectsAsync(cancellationToken);
            ITUnitCoverageProject[] tUnitCoverageProjects = await Task.WhenAll(tUnitProjects.Select(tUnitProject => _tUnitCoverageProjectFactory.CreateTUnitCoverageProjectAsync(tUnitProject, cancellationToken)));
            return tUnitCoverageProjects.Where(tp => tp.CoverageProject.Settings.Enabled).ToList();
        }

        private async Task CollectCoverageAsync(CoverageRun run)
        {
            await _executionGate.WaitAsync();
            CancellationToken cancellationToken = run.Token;
            bool raiseCoverageEndedMessage = true;
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                await LogCoverageStartingAsync();
                OnCollectingChanged(true); // order important
                _eventAggregator.SendMessage(new TestExecutionStartingMessage());
                _eventAggregator.SendMessage(new CoverageStartingMessage());
                List<ITUnitCoverageProject> tUnitCoverageProjects = await GetEnabledTUnitProjectsAsync(cancellationToken);
                if (tUnitCoverageProjects.Count != 0)
                {
                    bool success = await BuildAndCollectAsync(tUnitCoverageProjects, cancellationToken);
                    raiseCoverageEndedMessage = !success;
                }
                else
                {
                    await _logger.LogAsync("No enabled Microsoft.Testing.Platform test projects.");
                }
            }
            catch (OperationCanceledException)
            {
                await _logger.LogAsync("Coverage collection cancelled");
            }
            catch (Exception exc)
            {
                await _logger.LogAsync(exc.ToString());
            }
            finally
            {
                try
                {
                    bool isCurrent;
                    lock (_runLock)
                    {
                        isCurrent = ReferenceEquals(_activeRun, run);
                    }

                    if (raiseCoverageEndedMessage && isCurrent)
                    {
                        _eventAggregator.SendMessage(new CoverageEndedMessage());
                    }
                }
                finally
                {
                    FinishRunAndReleaseGate(run);
                }
            }
        }

        private async Task<bool> BuildAndCollectAsync(List<ITUnitCoverageProject> tUnitCoverageProjects, CancellationToken cancellationToken)
        {
            bool success = false;
            await _logger.LogAsync("Starting build");
            bool buildSuccess = await _buildHelper.BuildAsync(tUnitCoverageProjects.ConvertAll(tp => tp.VsHierarchy), cancellationToken);
            if (buildSuccess)
            {
                success = await CollectCoverageAsync(tUnitCoverageProjects, cancellationToken);
            }
            else
            {
                await _logger.LogAsync("Unsuccessful build.  Not collecting coverage");
            }

            return success;
        }

        private async Task<bool> CollectCoverageAsync(List<ITUnitCoverageProject> tUnitCoverageProjects, CancellationToken cancellationToken)
        {
            await _logger.LogAsync($"Collecting coverage for {tUnitCoverageProjects.Count} enabled Microsoft.Testing.Platform test projects");

            List<ICoverageProject> coverageProjects = tUnitCoverageProjects.ConvertAll(tUnitCoverageProject => tUnitCoverageProject.CoverageProject);
            cancellationToken.ThrowIfCancellationRequested();
            await _coverageToolOutputManager.SetProjectCoverageOutputFolderAsync(coverageProjects);

            bool runAllProjects = true;
            var coberturaFiles = new List<string>();
            foreach (ITUnitCoverageProject tUnitCoverageProject in tUnitCoverageProjects)
            {
                TUnitSettings tUnitSettings = await _tUnitSettingsProvider.ProvideAsync(tUnitCoverageProject, cancellationToken);
                bool success = await _tUnitCoverageRunner.RunAsync(tUnitSettings, tUnitCoverageProject.HasCoverageExtension, false, cancellationToken);
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
                cancellationToken.ThrowIfCancellationRequested();
                List<ProjectCoverageResult> results = coverageProjects.Select(project => new ProjectCoverageResult(
                    project, ProjectCoverageState.Collected, "Coverage collected by the manual MTP run.")).ToList();
                _fccEngine.RunAndProcessReport(new CoverageCollectionResult(coberturaFiles, coverageProjects, results));
            }
            else
            {
                await _logger.LogAsync("Not collecting coverage due to unsuccessful test");
            }

            return runAllProjects;
        }

        public async Task<IReadOnlyCollection<string>> PrepareForTestRunAsync()
        {
            CoverageRun run = BeginRun();
            await _executionGate.WaitAsync();
            CancellationToken cancellationToken = run.Token;
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                List<ITUnitCoverageProject> tUnitCoverageProjects = await GetEnabledTUnitProjectsAsync(cancellationToken);
                if (tUnitCoverageProjects.Count == 0)
                {
                    return Array.Empty<string>();
                }

                await _coverageToolOutputManager.SetProjectCoverageOutputFolderAsync(
                    tUnitCoverageProjects.ConvertAll(tUnitCoverageProject => tUnitCoverageProject.CoverageProject));

                await PrepareProjectsAsync(tUnitCoverageProjects, run);
                cancellationToken.ThrowIfCancellationRequested();
                run.Prepared = true;
                return run.Projects
                    .ConvertAll(preparedProject => preparedProject.TUnitCoverageProject.CoverageProject.ProjectFilePath);
            }
            catch (OperationCanceledException)
            {
                await _logger.LogAsync("Coverage preparation cancelled");
            }
            catch (Exception exc)
            {
                await _logger.LogAsync(exc.ToString());
            }
            finally
            {
                bool finish;
                lock (_runLock)
                {
                    run.Busy = false;
                    finish = !run.Prepared || run.Token.IsCancellationRequested || !ReferenceEquals(_activeRun, run);
                }

                try
                {
                    if (finish)
                    {
                        FinishRun(run);
                    }
                }
                finally
                {
                    _ = _executionGate.Release();
                }
            }

            return Array.Empty<string>();
        }

        private async Task PrepareProjectsAsync(
            List<ITUnitCoverageProject> tUnitCoverageProjects,
            CoverageRun run)
        {
            CancellationToken cancellationToken = run.Token;
            foreach (ITUnitCoverageProject tUnitCoverageProject in tUnitCoverageProjects)
            {
                cancellationToken.ThrowIfCancellationRequested();
                TUnitSettings tUnitSettings = await _tUnitSettingsProvider.ProvideAsync(tUnitCoverageProject, cancellationToken);
                TestConfigInjectionResult injection = null;
                if (tUnitCoverageProject.HasCoverageExtension)
                {
                    injection = await _testConfigInjector.InjectAsync(tUnitCoverageProject, tUnitSettings, cancellationToken);
                }
                else
                {
                    // a PackageReference cannot be added without a restore, so there is nothing to inject into
                    await _logger.LogAsync(
                        $"{tUnitCoverageProject.CoverageProject.ProjectName} - no Microsoft.Testing.Extensions.CodeCoverage package reference. Add it to enable coverage during the Test Explorer run.");
                }

                run.Projects.Add(new PreparedProject(tUnitCoverageProject, injection));
            }
        }

        public async Task<CoverageCollectionResult> CollectAfterTestRunAsync()
        {
            CoverageRun run;
            lock (_runLock)
            {
                run = _activeRun;
                if (run?.Prepared != true || run.Busy)
                {
                    return CoverageCollectionResult.Empty;
                }

                run.Busy = true;
            }

            await _executionGate.WaitAsync();
            CoverageCollectionResult result = CoverageCollectionResult.Empty;
            try
            {
                run.Token.ThrowIfCancellationRequested();
                OnCollectingChanged(true);
                result = await CollectPreparedAsync(run.Projects, run.Token);
            }
            catch (OperationCanceledException)
            {
                await _logger.LogAsync("Coverage collection cancelled");
            }
            catch (Exception exc)
            {
                await _logger.LogAsync(exc.ToString());
            }
            finally
            {
                FinishRunAndReleaseGate(run);
            }

            return result;
        }

        private async Task<CoverageCollectionResult> CollectPreparedAsync(List<PreparedProject> preparedProjects, CancellationToken cancellationToken)
        {
            var coverageFiles = new List<string>();
            var coverageProjects = new List<ICoverageProject>();
            var projectResults = new List<ProjectCoverageResult>();
            foreach (PreparedProject preparedProject in preparedProjects)
            {
                cancellationToken.ThrowIfCancellationRequested();
                coverageProjects.Add(preparedProject.TUnitCoverageProject.CoverageProject);
                if (preparedProject.Injection == null)
                {
                    string reason = preparedProject.TUnitCoverageProject.HasCoverageExtension
                        ? "Coverage configuration could not be injected. Check the platform version and testconfig.json in the Output pane."
                        : "Microsoft.Testing.Extensions.CodeCoverage is missing. Add the package or use manual MTP coverage.";
                    projectResults.Add(new ProjectCoverageResult(preparedProject.TUnitCoverageProject.CoverageProject, ProjectCoverageState.Skipped, reason));
                    await _logger.LogAsync(
                        $"{preparedProject.TUnitCoverageProject.CoverageProject.ProjectName} - skipping automatic coverage because options could not be injected. The Test Explorer selection cannot be preserved by re-running the test host. Use the manual MTP coverage command to run all enabled MTP projects.");
                    continue;
                }

                string coverageFile = await CollectInjectedAsync(preparedProject);
                if (coverageFile != null)
                {
                    coverageFiles.Add(coverageFile);
                }

                projectResults.Add(new ProjectCoverageResult(
                    preparedProject.TUnitCoverageProject.CoverageProject,
                    coverageFile == null ? ProjectCoverageState.Skipped : ProjectCoverageState.Collected,
                    coverageFile == null ? "No coverage file was produced; the project may not have been selected for this run." : "Coverage collected during the Test Explorer run."));
            }

            cancellationToken.ThrowIfCancellationRequested();
            return new CoverageCollectionResult(coverageFiles, coverageProjects, projectResults);
        }

        private async Task<string> CollectInjectedAsync(PreparedProject preparedProject)
        {
            string coverageOutputPath = preparedProject.Injection.CoverageOutputPath;
            if (_fileUtil.Exists(coverageOutputPath))
            {
                return coverageOutputPath;
            }

            // a single project not running - e.g. filtered out of the run - must not fail the others
            await _logger.LogAsync(
                $"{preparedProject.TUnitCoverageProject.CoverageProject.ProjectName} - no coverage at {coverageOutputPath}.  Skipping it - the project may not have taken part in the test run.");
            return null;
        }

        private void FinishRun(CoverageRun run)
        {
            bool notify;
            lock (_runLock)
            {
                notify = FinishRunCore(run);
            }

            if (!notify)
            {
                return;
            }

            OnCollectingChanged(false);
        }

        private bool FinishRunCore(CoverageRun run)
        {
            if (run.Finished)
            {
                return false;
            }

            run.Finished = true;
            foreach (PreparedProject project in run.Projects)
            {
                if (project.Injection != null)
                {
                    _testConfigInjector.Restore(project.Injection);
                }
            }

            run.Source.Dispose();
            bool isCurrent = ReferenceEquals(_activeRun, run);
            if (isCurrent)
            {
                _activeRun = null;
            }

            return isCurrent;
        }

        private void FinishRunAndReleaseGate(CoverageRun run)
        {
            try
            {
                FinishRun(run);
            }
            finally
            {
                _ = _executionGate.Release();
            }
        }
    }
}
