using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine;
using FineCodeCoverage.Engine.Messages;
using FineCodeCoverage.Impl.TestContainerDiscovery;
using FineCodeCoverage.Output;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly IFileUtil fileUtil;
        private readonly IEventAggregator eventAggregator;
        private readonly ILogger logger;
        private int coverageRunNumber = 1;
        private CancellationTokenSource cancellationTokenSource;
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
            IFileUtil fileUtil,
            IEventAggregator eventAggregator,
            ILogger logger
        )
        {
            buildHelper.ExternalBuildEvent += BuildHelper_ExternalBuildEvent;
            tUnitProjectsProvider.ReadyEvent += TUnitProjectsProvider_ReadyEvent;
            projectsProviderReady = tUnitProjectsProvider.Ready;
            tUnitCoverageRunner.ReadyEvent += TUnitRunner_ReadyEvent;
            this.tUnitProjectsProvider = tUnitProjectsProvider;
            this.buildHelper = buildHelper;
            this.tUnitCoverageProjectFactory = tUnitCoverageProjectFactory;
            this.tUnitCoverageRunner = tUnitCoverageRunner;
            this.coverageToolOutputManager = coverageToolOutputManager;
            this.fccEngine = fccEngine;
            this.fileUtil = fileUtil;
            this.eventAggregator = eventAggregator;
            this.logger = logger;
        }

        private void BuildHelper_ExternalBuildEvent(object sender, BuildStartEndArgs e)
        {
            externalBuildInProgress = e.IsStart;
            OnReady();
        }

        private void TUnitRunner_ReadyEvent(object sender, EventArgs e)
        {
            runnerReady = true;
            OnReady();
        }

        private void TUnitProjectsProvider_ReadyEvent(object sender, EventArgs e)
        {
            projectsProviderReady = tUnitProjectsProvider.Ready;
            OnReady();
        }

        public bool Ready => runnerReady && projectsProviderReady && !externalBuildInProgress;
        private void OnReady()
        {
            ReadyEvent?.Invoke(this, EventArgs.Empty);
        }

        protected void OnCollectingChanged(bool collecting)
        {
            CollectingChangedEvent?.Invoke(this, collecting);
        }

        public void Cancel()
        {
            if(cancellationTokenSource.IsCancellationRequested) return;
            cancellationTokenSource.Cancel();
        }

        public void CollectCoverage()
        {
            _ = Task.Run(async () => await CollectCoverageAsync());
        }

        private Task LogCoverageStartingAsync()
        {
            return logger.LogAsync(StatusMarkerProvider.Get($"Coverage Starting - {coverageRunNumber++}"));
        }

        private async Task<List<ITUnitCoverageProject>> GetEnabledTUnitProjectsAsync(CancellationToken cancellationToken)
        {
            var tUnitProjects = await tUnitProjectsProvider.GetTUnitProjectsAsync(cancellationToken);
            var tUnitCoverageProjects = await Task.WhenAll(tUnitProjects.Select(tUnitProject => tUnitCoverageProjectFactory.CreateTUnitCoverageProjectAsync(tUnitProject, cancellationToken)));
            return tUnitCoverageProjects.Where(tp => tp.CoverageProject.Settings.Enabled).ToList();
        }

        private async Task CollectCoverageAsync()
        {
            if(cancellationTokenSource != null)
            {
                return;
            }
            cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            await LogCoverageStartingAsync();
            eventAggregator.SendMessage(new NewReportMessage(null, null)); // clear existing report

            OnCollectingChanged(true);//order important
            eventAggregator.SendMessage(new CoverageStartingMessage());

            var raiseCoverageEndedMessage = true;
            try
            {
                var tUnitCoverageProjects = await GetEnabledTUnitProjectsAsync(cancellationToken);
                if (tUnitCoverageProjects.Any())
                {
                    var success = await BuildAndCollectAsync(tUnitCoverageProjects, cancellationToken);
                    raiseCoverageEndedMessage = !success;
                }
                else
                {
                    await logger.LogAsync("No enabled Tunit test projects.");
                }
            }
            catch(OperationCanceledException)
            {
                await logger.LogAsync("Coverage collection cancelled");
            }
            catch(Exception exc)
            {
                await logger.LogAsync(exc.ToString());
            }
            if (raiseCoverageEndedMessage)
            {
                eventAggregator.SendMessage(new CoverageEndedMessage());
            }
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
            OnCollectingChanged(false);
        }

        private async Task<bool> BuildAndCollectAsync(List<ITUnitCoverageProject> tUnitCoverageProjects, CancellationToken cancellationToken)
        {
            var success = false;
            await logger.LogAsync("Starting build");
            var buildSuccess = await buildHelper.BuildAsync(tUnitCoverageProjects.ConvertAll(tp => tp.VsHierarchy), cancellationToken);
            if (buildSuccess)
            {
                success = await CollectCoverageAsync(tUnitCoverageProjects, cancellationToken);
            }
            else
            {
                await logger.LogAsync("Unsuccessful build.  Not collecting coverage");
            }
            return success;
        }

        private async Task<TUnitSettings> GetTUnitSettingsAsync(ITUnitCoverageProject tUnitCoverageProject,CancellationToken cancellationToken)
        {
            await tUnitCoverageProject.CoverageProject.PrepareForCoverageAsync(cancellationToken,false);
            var coberturaPath = GetCoberturaPath(tUnitCoverageProject);
            var commandLineParseResult = tUnitCoverageProject.CommandLineParseResult;
            // todo commandLineParseResult.HasError
            string configurationPath = null;
            var additionalArgs = "";
            foreach (var option in commandLineParseResult.Options)
            {
                switch (option.Name)
                {
                    case "coverage":
                    case "coverage-output-format":
                    case "coverage-output"://for now will use own
                    break;
                    case "coverage-settings":
                    case "settings":
                        var arg = option.Arguments.SingleOrDefault();
                        if (arg != null)
                        {
                            arg = arg.Replace("\"", "").Replace("'","");
                            if (File.Exists(arg))
                            {
                                configurationPath = arg;
                            }
                        }
                        break;
                    default:
                        additionalArgs += $" {CommandLineParseResult.OptionPrefix}{option.Name} {string.Join(" ",option.Arguments)}";
                        break;
                }
            }

            if (configurationPath == null)
            {
                configurationPath = await WriteConfigurationAsync(tUnitCoverageProject, cancellationToken);
            }

            return new TUnitSettings(tUnitCoverageProject.ExePath, configurationPath, coberturaPath, additionalArgs);
        }

        private async Task<bool> CollectCoverageAsync(List<ITUnitCoverageProject> tUnitCoverageProjects, CancellationToken cancellationToken)
        {
            await logger.LogAsync($"Collecting coverage for {tUnitCoverageProjects.Count} enabled TUnit test projects with coverage extension");

            var coverageProjects = tUnitCoverageProjects.ConvertAll(tUnitCoverageProject => tUnitCoverageProject.CoverageProject);
            cancellationToken.ThrowIfCancellationRequested();
            await coverageToolOutputManager.SetProjectCoverageOutputFolderAsync(coverageProjects);

            var runAllProjects = true;
            List<string> coberturaFiles = new List<string>();
            foreach (var tUnitCoverageProject in tUnitCoverageProjects)
            {
                var tUnitSettings = await GetTUnitSettingsAsync(tUnitCoverageProject, cancellationToken);
                var success = await tUnitCoverageRunner.RunAsync(tUnitSettings, tUnitCoverageProject.HasCoverageExtension, false, cancellationToken);
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
                fccEngine.RunAndProcessReport(coberturaFiles.ToArray(), coverageProjects);
            }
            else
            {
                await logger.LogAsync("Not collecting coverage due to unsuccessful test");
            }
            return runAllProjects;
        }

        private static string GetCoberturaPath(ITUnitCoverageProject tUnitCoverageProject)
        {
            var coverageProject = tUnitCoverageProject.CoverageProject;
            return Path.Combine(coverageProject.CoverageOutputFolder, coverageProject.Id.ToString() + "coverage.xml");
        }

        private async Task<string> WriteConfigurationAsync(ITUnitCoverageProject tUnitCoverageProject, CancellationToken cancellationToken)
        {
            var coverageProject = tUnitCoverageProject.CoverageProject;
            var configurationPath = Path.Combine(coverageProject.CoverageOutputFolder, coverageProject.Id.ToString() + "config.xml");
            var configuration = await tUnitCoverageProject.GetConfigurationAsync(cancellationToken);
            fileUtil.WriteAllText(configurationPath, configuration);
            return configurationPath;
        }

        async Task<bool> ICoverageCollectableFromTestExplorer.IsCollectableAsync()
        {
            var tunitProjects =  await tUnitProjectsProvider.GetTUnitProjectsAsync(CancellationToken.None);
            return !tunitProjects.Any();
        }
    }
}
