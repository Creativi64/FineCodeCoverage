using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using NuGet.VisualStudio.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.ComponentModel.Composition;
using FineCodeCoverage.Output;
using System.Linq;
using System;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    [Export(typeof(ITUnitProjectsProvider))]
    internal class TUnitProjectsProvider : ITUnitProjectsProvider
    {
        private readonly ITestProjectsProvider testProjectsProvider;
        private readonly ITUnitInstalledPackagesService tUnitInstalledPackagesService;
        private readonly ITUnitProjectFactory tUnitProjectFactory;
        private readonly ILogger logger;
        private readonly ITUnitProjectCache tUnitProjectCache;
        private readonly Dictionary<InstalledPackageResultStatus, string> unsuccessfulNugetPackageResultLogs = new Dictionary<InstalledPackageResultStatus, string>
        {
            { InstalledPackageResultStatus.Unknown,"Nuget unknown status : Probably represents a bug in the method that created the result"},
            { InstalledPackageResultStatus.ProjectInvalid,"Nuget package invalid status: Package information could not be retrieved because the project is in an invalid state" },
            { InstalledPackageResultStatus.ProjectNotReady,"Nuget package project not ready: Please try again shortly" }
        };
        private bool initializedCache;

        [ImportingConstructor]
        public TUnitProjectsProvider(
            ITestProjectsProvider testProjectsProvider,
            ITUnitInstalledPackagesService tUnitInstalledPackagesService,
            ITUnitChangeNotifier tUnitChangeNotifier,
            ITUnitProjectFactory tUnitprojectFactory,
            ILogger logger,
            ITUnitProjectCache tUnitProjectCache
        )
        {
            tUnitChangeNotifier.ProjectAddedRemovedEvent += TUnitChangeNotifier_ProjectAddedRemovedEvent;
            tUnitChangeNotifier.PackageChangeEvent += TUnitChangeNotifier_PackageChangeEvent;
            tUnitChangeNotifier.SolutionClosedEvent += TUnitChangeNotifier_SolutionClosedEvent;
            this.testProjectsProvider = testProjectsProvider;
            this.tUnitInstalledPackagesService = tUnitInstalledPackagesService;
            this.tUnitProjectFactory = tUnitprojectFactory;
            this.logger = logger;
            this.tUnitProjectCache = tUnitProjectCache;
        }

        private void TUnitChangeNotifier_SolutionClosedEvent(object sender, System.EventArgs e)
        {
            if (initializedCache)
            {
                tUnitProjectCache.Clear();
                initializedCache = false;
            }
        }

        private void TUnitChangeNotifier_PackageChangeEvent(object sender, EventArgs _)
        {
            if (initializedCache)
            {
                tUnitProjectCache.Invalidate();
            }
        }

        private void TUnitChangeNotifier_ProjectAddedRemovedEvent(object sender, ProjectAddedRemoved e)
        {
            if (initializedCache && testProjectsProvider.IsTestProject(e.Project))
            {
                if (e.Added)
                {
                    tUnitProjectCache.Add(tUnitProjectFactory.Create(e.Project));
                } else
                {
                    tUnitProjectCache.Remove(e.Project);
                }
            }
        }

        public async Task<List<IVsHierarchy>> GetTUnitProjectsWithCoverageExtensionAsync()
        {
            if (!initializedCache)
            {
                var testProjects = await testProjectsProvider.ProvideAsync();
                var potentialTUnitProjects = testProjects.Select(tp => tUnitProjectFactory.Create(tp)).ToList();
                tUnitProjectCache.Initialize(potentialTUnitProjects);
                initializedCache = true;

            }
            var tUnitProjects = await tUnitProjectCache.GetTUnitProjectsAsync();
            return tUnitProjects.ConvertAll(tUnitProject => tUnitProject.Hierarchy);
        }
    }
}
