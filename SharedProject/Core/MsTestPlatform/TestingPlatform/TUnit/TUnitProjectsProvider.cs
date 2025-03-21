using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using FineCodeCoverage.Output;
using System.Linq;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    [Export(typeof(ITUnitProjectsProvider))]
    internal class TUnitProjectsProvider : ITUnitProjectsProvider
    {
        private readonly ITestProjectsProvider testProjectsProvider;
        private readonly ITUnitProjectFactory tUnitProjectFactory;
        private readonly ICPSProjectService cpsProjectService;
        private readonly ILogger logger;
        private readonly ITUnitProjectCache tUnitProjectCache;
        private bool initializedCache;

        [ImportingConstructor]
        public TUnitProjectsProvider(
            ITestProjectsProvider testProjectsProvider,
            ITUnitChangeNotifier tUnitChangeNotifier,
            ITUnitProjectFactory tUnitProjectFactory,
            ICPSProjectService vsProjectService,
            ILogger logger,
            ITUnitProjectCache tUnitProjectCache
        )
        {
            tUnitChangeNotifier.ProjectAddedRemovedEvent += TUnitChangeNotifier_ProjectAddedRemovedEvent;
            tUnitChangeNotifier.SolutionClosedEvent += TUnitChangeNotifier_SolutionClosedEvent;
            this.testProjectsProvider = testProjectsProvider;
            this.tUnitProjectFactory = tUnitProjectFactory;
            this.cpsProjectService = vsProjectService;
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

        private void TUnitChangeNotifier_ProjectAddedRemovedEvent(object sender, ProjectAddedRemoved e)
        {
            /*
                implementation is using project capability
                unlikely that the capability would change - could look at IProjectSnapshotWithCapabilitiesService / IProjectCapabilitiesScope ( ProjectServices.Capabilities )
            */
            if (initializedCache && testProjectsProvider.IsTestProject(e.Project))
            {
                if (e.Added)
                {
                    // should really align the configuration that build with the ConfiguredProject
                    // unlikely with a TUnit project that package references will depend on configuration
                    var cpsProject = cpsProjectService.GetProject(e.Project);
                    if(cpsProject != null)
                    {
                        tUnitProjectCache.Add(tUnitProjectFactory.Create(e.Project,cpsProject));
                    }
                } else
                {
                    tUnitProjectCache.Remove(e.Project);
                }
            }
        }

        public async Task<List<ITUnitProject>> GetTUnitProjectsAsync()
        {
            if (!initializedCache)
            {
                var potentialTUnitProjects = new List<ITUnitProject>();
                var testProjects = await testProjectsProvider.ProvideAsync();
                foreach (var testProject in testProjects)
                {
                    var cpsProject = await cpsProjectService.GetProjectAsync(testProject);
                    if (cpsProject != null)
                    {
                        var tUnitProject = tUnitProjectFactory.Create(testProject, cpsProject);
                        potentialTUnitProjects.Add(tUnitProject);
                    }
                }
                tUnitProjectCache.Initialize(potentialTUnitProjects);
                initializedCache = true;
            }

            return await tUnitProjectCache.GetTUnitProjectsAsync();
        }
    }
}
