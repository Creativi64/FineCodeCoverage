using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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
using FineCodeCoverage.VSAbstractions.OutputWindow;
using FineCodeCoverage.VSAbstractions.Threading;
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Collection.Runners
{
    [Export(typeof(ITUnitCoverage))]
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
        private int _coverageRunNumber = 1;
        private ICancellationTokenSource _cancellationTokenSource;
        private bool _runnerReady;
        private bool _projectsProviderReady;
        private bool _externalBuildInProgress;
        private List<PreparedProject> _preparedProjects;

        /// <summary>
        /// A project that took part in the Test Explorer run, and how its coverage is being collected.
        /// </summary>
        private sealed class PreparedProject
        {
            public PreparedProject(
                ITUnitCoverageProject tUnitCoverageProject,
                TUnitSettings settings,
                TestConfigInjectionResult injection)
            {
                TUnitCoverageProject = tUnitCoverageProject;
                Settings = settings;
                Injection = injection;
            }

            public ITUnitCoverageProject TUnitCoverageProject { get; }

            public TUnitSettings Settings { get; }

            /// <summary>
            /// Null when the coverage options could not be injected - the test host has to be re-run.
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
            ILogger logger)
        {
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
            try
            {
                if (_cancellationTokenSource?.IsCancellationRequested == false)
                {
                    _cancellationTokenSource.Cancel();
                }
            }
            catch (ObjectDisposedException)
            {
            }
        }

        public void CollectCoverage()
        {
            Cancel();
            _cancellationTokenSource = _disposeAwareTaskRunner.CreateLinkedTokenSource();
            _disposeAwareTaskRunner.RunAsyncFunc(CollectCoverageAsync);
        }

        private Task LogCoverageStartingAsync()
            => _logger.LogAsync(StatusMarkerProvider.Get($"Coverage Starting - {_coverageRunNumber++}"));

        private async Task<List<ITUnitCoverageProject>> GetEnabledTUnitProjectsAsync(CancellationToken cancellationToken)
        {
            List<ITUnitProject> tUnitProjects = await _tUnitProjectsProvider.GetTUnitProjectsAsync(cancellationToken);
            ITUnitCoverageProject[] tUnitCoverageProjects = await Task.WhenAll(tUnitProjects.Select(tUnitProject => _tUnitCoverageProjectFactory.CreateTUnitCoverageProjectAsync(tUnitProject, cancellationToken)));
            return tUnitCoverageProjects.Where(tp => tp.CoverageProject.Settings.Enabled).ToList();
        }

        private async Task CollectCoverageAsync()
        {
            CancellationToken cancellationToken = _cancellationTokenSource.Token;
            await LogCoverageStartingAsync();

            OnCollectingChanged(true); // order important
            _eventAggregator.SendMessage(new TestExecutionStartingMessage());
            _eventAggregator.SendMessage(new CoverageStartingMessage());

            bool raiseCoverageEndedMessage = true;
            try
            {
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

            if (raiseCoverageEndedMessage)
            {
                _eventAggregator.SendMessage(new CoverageEndedMessage());
            }

            DisposeCancellationTokenSource();
            OnCollectingChanged(false);
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
                _fccEngine.RunAndProcessReport(coberturaFiles.ToArray(), coverageProjects);
            }
            else
            {
                await _logger.LogAsync("Not collecting coverage due to unsuccessful test");
            }

            return runAllProjects;
        }

        public async Task<IReadOnlyCollection<string>> PrepareForTestRunAsync()
        {
            Cancel();
            _preparedProjects = null;
            _cancellationTokenSource = _disposeAwareTaskRunner.CreateLinkedTokenSource();
            CancellationToken cancellationToken = _cancellationTokenSource.Token;
            try
            {
                List<ITUnitCoverageProject> tUnitCoverageProjects = await GetEnabledTUnitProjectsAsync(cancellationToken);
                if (tUnitCoverageProjects.Count == 0)
                {
                    return Array.Empty<string>();
                }

                await _coverageToolOutputManager.SetProjectCoverageOutputFolderAsync(
                    tUnitCoverageProjects.ConvertAll(tUnitCoverageProject => tUnitCoverageProject.CoverageProject));

                _preparedProjects = await PrepareProjectsAsync(tUnitCoverageProjects, cancellationToken);
                return _preparedProjects
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

            return Array.Empty<string>();
        }

        private async Task<List<PreparedProject>> PrepareProjectsAsync(
            List<ITUnitCoverageProject> tUnitCoverageProjects,
            CancellationToken cancellationToken)
        {
            var preparedProjects = new List<PreparedProject>();
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
                        $"{tUnitCoverageProject.CoverageProject.ProjectName} - no Microsoft.Testing.Extensions.CodeCoverage package reference.  Coverage will be collected by re-running the test host.");
                }

                preparedProjects.Add(new PreparedProject(tUnitCoverageProject, tUnitSettings, injection));
            }

            return preparedProjects;
        }

        public async Task<CoverageCollectionResult> CollectAfterTestRunAsync()
        {
            List<PreparedProject> preparedProjects = _preparedProjects;
            _preparedProjects = null;
            if (preparedProjects == null || preparedProjects.Count == 0)
            {
                // nothing was prepared - preparation found no enabled projects, failed or was cancelled
                DisposeCancellationTokenSource();
                return CoverageCollectionResult.Empty;
            }

            OnCollectingChanged(true);
            CoverageCollectionResult result = CoverageCollectionResult.Empty;
            try
            {
                result = await CollectPreparedAsync(preparedProjects);
            }
            catch (OperationCanceledException)
            {
                await _logger.LogAsync("Coverage collection cancelled");
            }
            catch (Exception exc)
            {
                await _logger.LogAsync(exc.ToString());
            }

            DisposeCancellationTokenSource();
            OnCollectingChanged(false);
            return result;
        }

        private async Task<CoverageCollectionResult> CollectPreparedAsync(List<PreparedProject> preparedProjects)
        {
            CancellationToken cancellationToken = _cancellationTokenSource.Token;
            var coverageFiles = new List<string>();
            var coverageProjects = new List<ICoverageProject>();
            foreach (PreparedProject preparedProject in preparedProjects)
            {
                cancellationToken.ThrowIfCancellationRequested();
                coverageProjects.Add(preparedProject.TUnitCoverageProject.CoverageProject);
                string coverageFile = preparedProject.Injection != null
                    ? await CollectInjectedAsync(preparedProject)
                    : await CollectByRunningTestHostAsync(preparedProject, cancellationToken);
                if (coverageFile != null)
                {
                    coverageFiles.Add(coverageFile);
                }
            }

            return new CoverageCollectionResult(coverageFiles, coverageProjects);
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

        private async Task<string> CollectByRunningTestHostAsync(PreparedProject preparedProject, CancellationToken cancellationToken)
        {
            bool success = await _tUnitCoverageRunner.RunAsync(
                preparedProject.Settings,
                preparedProject.TUnitCoverageProject.HasCoverageExtension,
                false,
                cancellationToken);
            if (success)
            {
                return preparedProject.Settings.OutputPath;
            }

            await _logger.LogAsync(
                $"{preparedProject.TUnitCoverageProject.CoverageProject.ProjectName} - unsuccessful test host run.  No coverage.");
            return null;
        }

        private void DisposeCancellationTokenSource()
        {
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }
}
