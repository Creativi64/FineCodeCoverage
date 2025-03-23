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
        private readonly ITUnitRunner tUnitRunner;
        private readonly ICoverageToolOutputManager coverageToolOutputManager;
        private readonly IFCCEngine fccEngine;
        private readonly IFileUtil fileUtil;
        private readonly IEventAggregator eventAggregator;
        private readonly ILogger logger;
        private int coverageRunNumber = 1;
        private CancellationTokenSource cancellationTokenSource;

        public event EventHandler ReadyEvent;

        [ImportingConstructor]
        public TUnitCoverage(
            ITUnitProjectsProvider tUnitProjectsProvider,
            IBuildHelper buildHelper,
            ITUnitCoverageProjectFactory tUnitCoverageProjectFactory,
            ITUnitRunner tUnitRunner,
            ICoverageToolOutputManager coverageToolOutputManager,
            IFCCEngine fccEngine,
            IFileUtil fileUtil,
            IEventAggregator eventAggregator,
            ILogger logger

        )
        {
            tUnitProjectsProvider.ReadyEvent += TUnitProjectsProvider_ReadyEvent;
            this.tUnitProjectsProvider = tUnitProjectsProvider;
            this.buildHelper = buildHelper;
            this.tUnitCoverageProjectFactory = tUnitCoverageProjectFactory;
            this.tUnitRunner = tUnitRunner;
            this.coverageToolOutputManager = coverageToolOutputManager;
            this.fccEngine = fccEngine;
            this.fileUtil = fileUtil;
            this.eventAggregator = eventAggregator;
            this.logger = logger;
        }

        private void TUnitProjectsProvider_ReadyEvent(object sender, EventArgs e)
        {
            ReadyEvent?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<bool> CollectingChangedEvent;

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

        private async Task<List<ITUnitCoverageProject>> GetEnabledTUnitCoverageProjectsAsync(CancellationToken cancellationToken)
        {
            var tUnitProjects = await tUnitProjectsProvider.GetTUnitProjectsAsync(cancellationToken);
            var tUnitProjectHierarchies = tUnitProjects.Where(tp => tp.HasCoverageExtension).Select(tp => tp.Hierarchy).ToList();
            var tUnitCoverageProjects = await Task.WhenAll(tUnitProjectHierarchies.Select(tUnitProject => tUnitCoverageProjectFactory.CreateCoverageProjectAsync(tUnitProject,cancellationToken)));
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
                var tUnitCoverageProjects = await GetEnabledTUnitCoverageProjectsAsync(cancellationToken);
                if (tUnitCoverageProjects.Any())
                {
                    await logger.LogAsync("Starting build");
                    var buildSuccess = await buildHelper.BuildAsync(tUnitCoverageProjects.ConvertAll(tp => tp.VsHierarchy),cancellationToken);
                    if (buildSuccess)
                    {
                        await logger.LogAsync($"Collecting coverage for {tUnitCoverageProjects.Count} enabled TUnit test projects with coverage extension");

                        var coverageProjects = tUnitCoverageProjects.ConvertAll(tUnitCoverageProject => tUnitCoverageProject.CoverageProject);
                        cancellationToken.ThrowIfCancellationRequested();
                        await coverageToolOutputManager.SetProjectCoverageOutputFolderAsync(coverageProjects);

                        var runAllProjects = true;
                        List<string> coberturaFiles = new List<string>();
                        foreach (var tUnitCoverageProject in tUnitCoverageProjects)
                        {
                            var coverageProject = tUnitCoverageProject.CoverageProject;
                            await coverageProject.PrepareForCoverageAsync(cancellationToken, false);
                            var configurationPath = Path.Combine(coverageProject.CoverageOutputFolder, coverageProject.Id.ToString() + "config.xml");
                            //fileUtil.WriteAllText(configurationPath, tUnitCoverageProject.Configuration);                        
                            var coberturaPath = Path.Combine(coverageProject.CoverageOutputFolder, coverageProject.Id.ToString() + "coverage.xml");
                            await Task.Yield();//todo was this how to get off ui thread ?
                            var success = await tUnitRunner.RunAsync(tUnitCoverageProject.ExePath, configurationPath, coberturaPath,false, cancellationToken);
                            if (success)
                            {
                                coberturaFiles.Add(coberturaPath);
                            }
                            else
                            {
                                // show message box
                                runAllProjects = false;
                            }
                        }

                        if (runAllProjects)
                        {
                            raiseCoverageEndedMessage = false;
                            fccEngine.RunAndProcessReport(coberturaFiles.ToArray(), coverageProjects);
                        }
                    }
                    else
                    {
                        await logger.LogAsync("Unsuccessful build.  Not collecting coverage");
                    }
                }
                else
                {
                    await logger.LogAsync("No enabled Tunit test projects with Microsoft.Testing.Extensions.CodeCoverage");
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

        async Task<bool> ICoverageCollectableFromTestExplorer.IsCollectableAsync()
        {
            var tunitProjects =  await tUnitProjectsProvider.GetTUnitProjectsAsync(CancellationToken.None);
            return !tunitProjects.Any();
        }
    }
}
