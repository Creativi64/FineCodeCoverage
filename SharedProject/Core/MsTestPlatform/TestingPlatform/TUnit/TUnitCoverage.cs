using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using Task = System.Threading.Tasks.Task;


namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    [Export(typeof(ITUnitCoverage))]
    internal class TUnitCoverage : ITUnitCoverage
    {
        private readonly ITUnitProjectsProvider tUnitProjectsProvider;
        private readonly IBuildHelper buildHelper;
        private readonly ITUnitCoverageProjectFactory tUnitCoverageProjectFactory;
        private readonly ITUnitRunner tUnitRunner;
        private readonly ICoverageToolOutputManager coverageToolOutputManager;
        private readonly IFCCEngine fccEngine;
        private readonly IFileUtil fileUtil;

        [ImportingConstructor]
        public TUnitCoverage(
            ITUnitProjectsProvider tUnitProjectsProvider,
            IBuildHelper buildHelper,
            ITUnitCoverageProjectFactory tUnitCoverageProjectFactory,
            ITUnitRunner tUnitRunner,
            ICoverageToolOutputManager coverageToolOutputManager,
            IFCCEngine fccEngine,
            IFileUtil fileUtil

        )
        {
            this.tUnitProjectsProvider = tUnitProjectsProvider;
            this.buildHelper = buildHelper;
            this.tUnitCoverageProjectFactory = tUnitCoverageProjectFactory;
            this.tUnitRunner = tUnitRunner;
            this.coverageToolOutputManager = coverageToolOutputManager;
            this.fccEngine = fccEngine;
            this.fileUtil = fileUtil;
        }
        public void CollectCoverage()
        {
            _ = Task.Run(async () => await CollectCoverageAsync());
        }

        private async Task CollectCoverageAsync()
        {
            var tUnitProjects = await tUnitProjectsProvider.GetTUnitProjectsWithCoverageExtensionAsync();
            if (tUnitProjects.Any())
            {
                var buildSuccess = await buildHelper.BuildInDebugConfigAsync(tUnitProjects);
                if (buildSuccess)
                {
                    var tUnitCoverageProjects = await Task.WhenAll(tUnitProjects.Select(tUnitProject => tUnitCoverageProjectFactory.CreateCoverageProjectAsync(tUnitProject)));
                    var coverageProjects = tUnitCoverageProjects.Select(tUnitCoverageProject => tUnitCoverageProject.CoverageProject).ToList();
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
                        fccEngine.RunAndProcessReport(coberturaFiles.ToArray(), coverageProjects);
                    }
                }
                else
                {
                    //todo - show a message box ?
                }
            }
            else
            {
                // todo - show a message box ?
            }

        }
    }
}
