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
            tUnitChangeNotifier.ProjectAddedRemovedEvent += TUnitChangeNotifier_ProjectAddedRemovedEvent;
            tUnitChangeNotifier.SolutionClosedEvent += TUnitChangeNotifier_SolutionClosedEvent;
            tUnitChangeNotifier.SolutionOpenedEvent += TUnitChangeNotifier_SolutionOpenedEvent;
            _solutionProjectsProvider = solutionProjectsProvider;
            _cpsTestProjectService = cpsTestProjectService;
            _tUnitProjectFactory = tUnitProjectFactory;
            _tUnitProjectCache = tUnitProjectCache;
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                bool solutionOpen = await solutionProjectsProvider.IsSolutionOpenAsync();
                if (!solutionOpen)
                {
                    return;
                }

                OnReady(true);
            });
        }

        public bool Ready { get; private set; }

        private void OnReady(bool ready)
        {
            Ready = ready;
            ReadyEvent?.Invoke(this, EventArgs.Empty);
        }

        private void TUnitChangeNotifier_SolutionOpenedEvent(object sender, EventArgs e) => OnReady(true);

        private void TUnitChangeNotifier_SolutionClosedEvent(object sender, EventArgs e)
        {
            _addedProjects.Clear();
            if (_initializedCache)
            {
                _tUnitProjectCache.Clear();
                _initializedCache = false;
            }

            OnReady(false);
        }

        private void TUnitChangeNotifier_ProjectAddedRemovedEvent(object sender, ProjectAddedRemoved e)
        {
            if (!_initializedCache)
            {
                return;
            }

            IVsHierarchy project = e.Project;
            if (e.Added)
            {
                _addedProjects.Add(project);
            }
            else
            {
                bool removed = _addedProjects.Remove(project);
                if (!removed)
                {
                    _tUnitProjectCache.Remove(e.Project);
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
            var cpsTestProjectsAndHierarchys = new List<CpsProjectAndHierarchy>();
            foreach (IVsHierarchy project in projects)
            {
                ConfiguredProject cpsTestProject = await _cpsTestProjectService.GetProjectAsync(project);
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
            List<CpsProjectAndHierarchy> cpsTestProjectAndHierarchys = await GetCpsTestProjectsAndHierarchysAsync(projects);
            foreach (CpsProjectAndHierarchy cpsTestProjectAndHierarchy in cpsTestProjectAndHierarchys)
            {
                ITUnitProject tUnitProject = _tUnitProjectFactory.Create(cpsTestProjectAndHierarchy.Hierarchy, cpsTestProjectAndHierarchy.CpsProject);
                potentialTUnitProjects.Add(tUnitProject);
            }

            return potentialTUnitProjects;
        }

        public async Task<List<ITUnitProject>> GetTUnitProjectsAsync(CancellationToken cancellationToken)
        {
            if (!_initializedCache)
            {
                List<IVsHierarchy> solutionProjects = await _solutionProjectsProvider.GetLoadedProjectsAsync(cancellationToken);
                List<ITUnitProject> potentialTUnitProjects = await GetTUnitProjectsAsync(solutionProjects);
                _tUnitProjectCache.Initialize(potentialTUnitProjects);
                _initializedCache = true;
            }
            else
            {
                List<ITUnitProject> newTUnitProjects = await GetTUnitProjectsAsync(_addedProjects);
                foreach (ITUnitProject newTUnitProject in newTUnitProjects)
                {
                    _tUnitProjectCache.Add(newTUnitProject);
                }

                _addedProjects.Clear();
            }

            return await _tUnitProjectCache.GetTUnitProjectsAsync(cancellationToken);
        }
    }
}
