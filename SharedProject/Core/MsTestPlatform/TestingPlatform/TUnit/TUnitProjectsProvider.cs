using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    [Export(typeof(ITUnitProjectsProvider))]
    internal class TUnitProjectsProvider : ITUnitProjectsProvider
    {
        private readonly ISolutionProjectsProvider solutionProjectsProvider;
        private readonly ICPSTestProjectService cpsTestProjectService;
        private readonly ITUnitChangeNotifier tUnitChangeNotifier;
        private readonly ITUnitProjectFactory tUnitProjectFactory;
        private readonly ITUnitProjectCache tUnitProjectCache;
        private bool initializedCache;
        private readonly List<IVsHierarchy> addedProjects = new List<IVsHierarchy>();

        public event EventHandler ReadyEvent;

        [ImportingConstructor]
        public TUnitProjectsProvider(
            ISolutionProjectsProvider solutionProjectsProvider,
            ICPSTestProjectService cpsTestProjectService,
            ITUnitChangeNotifier tUnitChangeNotifier,
            ITUnitProjectFactory tUnitProjectFactory,
            ITUnitProjectCache tUnitProjectCache
        )
        {
            tUnitChangeNotifier.ProjectAddedRemovedEvent += TUnitChangeNotifier_ProjectAddedRemovedEvent;
            tUnitChangeNotifier.SolutionClosedEvent += TUnitChangeNotifier_SolutionClosedEvent;
            tUnitChangeNotifier.SolutionOpenedEvent += TUnitChangeNotifier_SolutionOpenedEvent;
            this.solutionProjectsProvider = solutionProjectsProvider;
            this.cpsTestProjectService = cpsTestProjectService;
            this.tUnitChangeNotifier = tUnitChangeNotifier;
            this.tUnitProjectFactory = tUnitProjectFactory;
            this.tUnitProjectCache = tUnitProjectCache;
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                bool solutionOpen = await solutionProjectsProvider.IsSolutionOpenAsync();
                if (solutionOpen)
                {
                    OnReady(true);
                }
            });
        }

        public bool Ready { get; private set; }

        private void OnReady(bool ready)
        {
            Ready = ready;
            ReadyEvent?.Invoke(this, EventArgs.Empty);
        }

        private void TUnitChangeNotifier_SolutionOpenedEvent(object sender, EventArgs e)
        {
            OnReady(true);
        }

        private void TUnitChangeNotifier_SolutionClosedEvent(object sender, EventArgs e)
        {
            addedProjects.Clear();
            if (initializedCache)
            {
                tUnitProjectCache.Clear();
                initializedCache = false;
            }
            OnReady(false);
        }

        private void TUnitChangeNotifier_ProjectAddedRemovedEvent(object sender, ProjectAddedRemoved e)
        {
            if (initializedCache)
            {
                IVsHierarchy project = e.Project;
                if (e.Added)
                {
                    addedProjects.Add(project);
                }
                else
                {
                    bool removed = addedProjects.Remove(project);
                    if (!removed)
                    {
                        tUnitProjectCache.Remove(e.Project);
                    }
                }
            }
        }

        private class CpsProjectAndHierarchy
        {
            public CpsProjectAndHierarchy(ConfiguredProject cpsProject, IVsHierarchy hierarchy)
            {
                CpsProject = cpsProject;
                Hierarchy = hierarchy;
            }

            public ConfiguredProject CpsProject { get; }
            public IVsHierarchy Hierarchy { get; }
        }

        private async Task<List<CpsProjectAndHierarchy>> GetCpsTestProjectsAndHierarchysAsync(IEnumerable<IVsHierarchy> projects)
        {
            List<CpsProjectAndHierarchy> cpsTestProjectsAndHierarchys = new List<CpsProjectAndHierarchy>();
            foreach (IVsHierarchy project in projects)
            {
                ConfiguredProject cpsTestProject = await cpsTestProjectService.GetProjectAsync(project);
                if (cpsTestProject != null)
                {
                    cpsTestProjectsAndHierarchys.Add(new CpsProjectAndHierarchy(cpsTestProject, project));
                }
            }
            return cpsTestProjectsAndHierarchys;
        }

        private async Task<List<ITUnitProject>> GetTUnitProjectsAsync(IEnumerable<IVsHierarchy> projects)
        {
            List<ITUnitProject> potentialTUnitProjects = new List<ITUnitProject>();
            List<CpsProjectAndHierarchy> cpsTestProjectAndHierarchys = await GetCpsTestProjectsAndHierarchysAsync(projects);
            foreach (CpsProjectAndHierarchy cpsTestProjectAndHierarchy in cpsTestProjectAndHierarchys)
            {
                ITUnitProject tUnitProject = tUnitProjectFactory.Create(cpsTestProjectAndHierarchy.Hierarchy, cpsTestProjectAndHierarchy.CpsProject);
                potentialTUnitProjects.Add(tUnitProject);
            }
            return potentialTUnitProjects;
        }

        public async Task<List<ITUnitProject>> GetTUnitProjectsAsync(CancellationToken cancellationToken)
        {
            if (!initializedCache)
            {
                List<IVsHierarchy> solutionProjects = await solutionProjectsProvider.GetLoadedProjectsAsync(cancellationToken);
                List<ITUnitProject> potentialTUnitProjects = await GetTUnitProjectsAsync(solutionProjects);
                tUnitProjectCache.Initialize(potentialTUnitProjects);
                initializedCache = true;
            }
            else
            {
                List<ITUnitProject> newTUnitProjects = await GetTUnitProjectsAsync(addedProjects);
                foreach (ITUnitProject newTUnitProject in newTUnitProjects)
                {
                    tUnitProjectCache.Add(newTUnitProject);
                }
                addedProjects.Clear();
            }

            return await tUnitProjectCache.GetTUnitProjectsAsync(cancellationToken);
        }
    }
}
