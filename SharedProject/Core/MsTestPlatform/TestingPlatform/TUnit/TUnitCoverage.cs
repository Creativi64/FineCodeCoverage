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
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    [Export(typeof(ITUnitCoverage))]
    [Export(typeof(ICoverageCollectableFromTestExplorer))]

    internal class TUnitCoverage : ITUnitCoverage, ICoverageCollectableFromTestExplorer, IListener<CoverageStartingMessage>, IListener<CoverageEndedMessage>
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
            eventAggregator.AddListener(this);
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

        public event EventHandler<bool> EnabledChanged;
        public event EventHandler<bool> CollectingChanged;

        protected void OnEnabledChanged(bool enabled)
        {
            EnabledChanged?.Invoke(this, enabled);
        }

        protected void OnCollectingChanged(bool collecting)
        {
            isCollecting = collecting;
            CollectingChanged?.Invoke(this, collecting);
        }

        public void Cancel()
        {
            // will store a cancellation token that pass to each method
        }

        private bool isCollecting;
        public void CollectCoverage()
        {
            _ = Task.Run(async () => await CollectCoverageAsync());
        }

        private Task LogCoverageStartingAsync()
        {
            return logger.LogAsync(StatusMarkerProvider.Get($"Coverage Starting - {coverageRunNumber++}"));
        }

        private async Task CollectCoverageAsync()
        {
            await LogCoverageStartingAsync();
            eventAggregator.SendMessage(new NewReportMessage(null, null)); // clear existing report

            OnCollectingChanged(true);//order important
            eventAggregator.SendMessage(new CoverageStartingMessage());

            var raiseCoverageEndedMessage = true;
            try
            {
                var tUnitProjects = await tUnitProjectsProvider.GetTUnitProjectsWithCoverageExtensionAsync();
                if (tUnitProjects.Any())
                {
                    await logger.LogAsync("Starting build");
                    var buildSuccess = await buildHelper.BuildInDebugConfigAsync(tUnitProjects);
                    if (buildSuccess)
                    {
                        var tUnitCoverageProjects = await Task.WhenAll(tUnitProjects.Select(tUnitProject => tUnitCoverageProjectFactory.CreateCoverageProjectAsync(tUnitProject)));
                        var coverageProjects = tUnitCoverageProjects.Select(tUnitCoverageProject => tUnitCoverageProject.CoverageProject).ToList();

                        await logger.LogAsync($"Collecting coverage for {coverageProjects.Count} TUnit test projects with coverage extension");
                        var runAllProjects = true;
                        List<string> coberturaFiles = new List<string>();
                        await coverageToolOutputManager.SetProjectCoverageOutputFolderAsync(coverageProjects);
                        foreach (var tUnitCoverageProject in tUnitCoverageProjects)
                        {
                            var coverageProject = tUnitCoverageProject.CoverageProject;
                            await coverageProject.PrepareForCoverageAsync(CancellationToken.None, false);
                            var configurationPath = Path.Combine(coverageProject.CoverageOutputFolder, coverageProject.Id.ToString() + "config.xml");
                            //fileUtil.WriteAllText(configurationPath, tUnitCoverageProject.Configuration);                        
                            var coberturaPath = Path.Combine(coverageProject.CoverageOutputFolder, coverageProject.Id.ToString() + "coverage.xml");
                            await Task.Yield();//todo was this how to get off ui thread ?
                            var success = await tUnitRunner.RunAsync(tUnitCoverageProject.ExePath, configurationPath, coberturaPath);
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
                    await logger.LogAsync("No Tunit test projects with Microsoft.Testing.Extensions.CodeCoverage");
                }
            }catch(Exception exc)
            {
                await logger.LogAsync(exc.ToString());
            }
            if (raiseCoverageEndedMessage)
            {
                eventAggregator.SendMessage(new CoverageEndedMessage());
            }
            OnCollectingChanged(false);
        }

        public void Handle(CoverageStartingMessage message)
        {
            if (!isCollecting)
            {
                OnEnabledChanged(false);
            }
        }

        public void Handle(CoverageEndedMessage message)
        {
            OnEnabledChanged(true);
        }

        async System.Threading.Tasks.Task<bool> ICoverageCollectableFromTestExplorer.IsCollectableAsync()
        {
            var tunitProjects =  await tUnitProjectsProvider.GetTUnitProjectsWithCoverageExtensionAsync();
            return !tunitProjects.Any();
        }
    }
}
