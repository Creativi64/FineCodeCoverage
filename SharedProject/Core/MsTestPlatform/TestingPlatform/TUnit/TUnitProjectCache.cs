using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell.Interop;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    [Export(typeof(ITUnitProjectCache))]
    internal class TUnitProjectCache : ITUnitProjectCache
    {
        private Dictionary<IVsHierarchy, ITUnitProject> projectLookup;
        public void Add(ITUnitProject tUnitProject) => this.projectLookup.Add(tUnitProject.Hierarchy, tUnitProject);

        public void Clear()
        {
            foreach (ITUnitProject tUnitproject in this.projectLookup.Values)
            {
                tUnitproject.Dispose();
            }

            this.projectLookup = null;
        }

        public async Task<List<ITUnitProject>> GetTUnitProjectsAsync(CancellationToken cancellationToken)
        {
            var tUnitProjects = new List<ITUnitProject>();
            foreach (ITUnitProject project in this.projectLookup.Values)
            {
                await project.UpdateStateAsync(cancellationToken);
                if (project.IsTUnit)
                {
                    tUnitProjects.Add(project);
                }
            }

            return tUnitProjects;

        }

        public void Initialize(List<ITUnitProject> tUnitProjects)
            => this.projectLookup = tUnitProjects.ToDictionary(p => p.Hierarchy);

        public void Remove(IVsHierarchy project)
        {
            this.projectLookup[project].Dispose();
            _ = this.projectLookup.Remove(project);
        }
    }
}
