using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    [Export(typeof(ITUnitProjectCache))]
    internal class TUnitProjectCache : ITUnitProjectCache
    {
        private Dictionary<IVsHierarchy, ITUnitProject> projectLookup;
        private bool invalid;
        public void Add(ITUnitProject tUnitProject)
        {
            projectLookup.Add(tUnitProject.Hierarchy, tUnitProject);
        }

        public void Clear()
        {
            projectLookup = null;
            invalid = false;
        }

        public async Task<List<ITUnitProject>> GetTUnitProjectsAsync()
        {
            var tUnitProjects = new List<ITUnitProject>();
            foreach (var project in projectLookup.Values)
            {
                await project.UpdateStateAsync(invalid);
                if (project.IsTUnit)
                {
                    tUnitProjects.Add(project);
                }
            }
            invalid = false;
            return tUnitProjects;

        }

        public void Initialize(List<ITUnitProject> tUnitProjects)
        {
            projectLookup = tUnitProjects.ToDictionary(p => p.Hierarchy);
        }

        public void Invalidate()
        {
            invalid = true;
        }

        public void Remove(IVsHierarchy project)
        {
            projectLookup.Remove(project);
        }
    }
}
