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
        private readonly ISolutionProjectsProvider _solutionProjectsProvider;
        private readonly ICPSTestProjectService _cpsTestProjectService;
        private readonly ITUnitProjectFactory _tUnitProjectFactory;
        private readonly ITUnitProjectCache _tUnitProjectCache;
        private bool _initializedCache;
        private readonly List<IVsHierarchy> _addedProjects = new List<IVsHierarchy>();

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
            this._solutionProjectsProvider = solutionProjectsProvider;
            this._cpsTestProjectService = cpsTestProjectService;
            this._tUnitProjectFactory = tUnitProjectFactory;
            this._tUnitProjectCache = tUnitProjectCache;
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                bool solutionOpen = await solutionProjectsProvider.IsSolutionOpenAsync();
                if (!solutionOpen)
                {
                    return;
                }

                this.OnReady(true);
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
            this._addedProjects.Clear();
            if (this._initializedCache)
            {
                this._tUnitProjectCache.Clear();
                this._initializedCache = false;
            }

            this.OnReady(false);
        }

        private void TUnitChangeNotifier_ProjectAddedRemovedEvent(object sender, ProjectAddedRemoved e)
        {
            if (!this._initializedCache)
            {
                return;
            }

            IVsHierarchy project = e.Project;
            if (e.Added)
            {
                this._addedProjects.Add(project);
            }
            else
            {
                bool removed = this._addedProjects.Remove(project);
                if (!removed)
                {
                    this._tUnitProjectCache.Remove(e.Project);
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
                ConfiguredProject cpsTestProject = await this._cpsTestProjectService.GetProjectAsync(project);
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
                ITUnitProject tUnitProject = this._tUnitProjectFactory.Create(cpsTestProjectAndHierarchy.Hierarchy, cpsTestProjectAndHierarchy.CpsProject);
                potentialTUnitProjects.Add(tUnitProject);
            }

            return potentialTUnitProjects;
        }

        public async Task<List<ITUnitProject>> GetTUnitProjectsAsync(CancellationToken cancellationToken)
        {
            if (!this._initializedCache)
            {
                List<IVsHierarchy> solutionProjects = await this._solutionProjectsProvider.GetLoadedProjectsAsync(cancellationToken);
                List<ITUnitProject> potentialTUnitProjects = await this.GetTUnitProjectsAsync(solutionProjects);
                this._tUnitProjectCache.Initialize(potentialTUnitProjects);
                this._initializedCache = true;
            }
            else
            {
                List<ITUnitProject> newTUnitProjects = await this.GetTUnitProjectsAsync(this._addedProjects);
                foreach (ITUnitProject newTUnitProject in newTUnitProjects)
                {
                    this._tUnitProjectCache.Add(newTUnitProject);
                }

                this._addedProjects.Clear();
            }

            return await this._tUnitProjectCache.GetTUnitProjectsAsync(cancellationToken);
        }
    }
}