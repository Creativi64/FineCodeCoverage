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
            tUnitChangeNotifier.ProjectAddedRemovedEvent += this.TUnitChangeNotifier_ProjectAddedRemovedEvent;
            tUnitChangeNotifier.SolutionClosedEvent += this.TUnitChangeNotifier_SolutionClosedEvent;
            tUnitChangeNotifier.SolutionOpenedEvent += this.TUnitChangeNotifier_SolutionOpenedEvent;
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
                    this.OnReady(true);
                }
            });
        }

        public bool Ready { get; private set; }

        private void OnReady(bool ready)
        {
            this.Ready = ready;
            ReadyEvent?.Invoke(this, EventArgs.Empty);
        }

        private void TUnitChangeNotifier_SolutionOpenedEvent(object sender, EventArgs e) => this.OnReady(true);

        private void TUnitChangeNotifier_SolutionClosedEvent(object sender, EventArgs e)
        {
            this.addedProjects.Clear();
            if (this.initializedCache)
            {
                this.tUnitProjectCache.Clear();
                this.initializedCache = false;
            }

            this.OnReady(false);
        }

        private void TUnitChangeNotifier_ProjectAddedRemovedEvent(object sender, ProjectAddedRemoved e)
        {
            if (this.initializedCache)
            {
                IVsHierarchy project = e.Project;
                if (e.Added)
                {
                    this.addedProjects.Add(project);
                }
                else
                {
                    bool removed = this.addedProjects.Remove(project);
                    if (!removed)
                    {
                        this.tUnitProjectCache.Remove(e.Project);
                    }
                }
            }
        }

        private class CpsProjectAndHierarchy
        {
            public CpsProjectAndHierarchy(ConfiguredProject cpsProject, IVsHierarchy hierarchy)
            {
                this.CpsProject = cpsProject;
                this.Hierarchy = hierarchy;
            }

            public ConfiguredProject CpsProject { get; }
            public IVsHierarchy Hierarchy { get; }
        }

        private async Task<List<CpsProjectAndHierarchy>> GetCpsTestProjectsAndHierarchysAsync(IEnumerable<IVsHierarchy> projects)
        {
            var cpsTestProjectsAndHierarchys = new List<CpsProjectAndHierarchy>();
            foreach (IVsHierarchy project in projects)
            {
                ConfiguredProject cpsTestProject = await this.cpsTestProjectService.GetProjectAsync(project);
                if (cpsTestProject != null)
                {
                    cpsTestProjectsAndHierarchys.Add(new CpsProjectAndHierarchy(cpsTestProject, project));
                }
            }

            return cpsTestProjectsAndHierarchys;
        }

        private async Task<List<ITUnitProject>> GetTUnitProjectsAsync(IEnumerable<IVsHierarchy> projects)
        {
            var potentialTUnitProjects = new List<ITUnitProject>();
            List<CpsProjectAndHierarchy> cpsTestProjectAndHierarchys = await this.GetCpsTestProjectsAndHierarchysAsync(projects);
            foreach (CpsProjectAndHierarchy cpsTestProjectAndHierarchy in cpsTestProjectAndHierarchys)
            {
                ITUnitProject tUnitProject = this.tUnitProjectFactory.Create(cpsTestProjectAndHierarchy.Hierarchy, cpsTestProjectAndHierarchy.CpsProject);
                potentialTUnitProjects.Add(tUnitProject);
            }

            return potentialTUnitProjects;
        }

        public async Task<List<ITUnitProject>> GetTUnitProjectsAsync(CancellationToken cancellationToken)
        {
            if (!this.initializedCache)
            {
                List<IVsHierarchy> solutionProjects = await this.solutionProjectsProvider.GetLoadedProjectsAsync(cancellationToken);
                List<ITUnitProject> potentialTUnitProjects = await this.GetTUnitProjectsAsync(solutionProjects);
                this.tUnitProjectCache.Initialize(potentialTUnitProjects);
                this.initializedCache = true;
            }
            else
            {
                List<ITUnitProject> newTUnitProjects = await this.GetTUnitProjectsAsync(this.addedProjects);
                foreach (ITUnitProject newTUnitProject in newTUnitProjects)
                {
                    this.tUnitProjectCache.Add(newTUnitProject);
                }

                this.addedProjects.Clear();
            }

            return await this.tUnitProjectCache.GetTUnitProjectsAsync(cancellationToken);
        }
    }
}