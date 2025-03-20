using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    internal interface ITUnitProjectCache
    {
        void Invalidate();

        void Initialize(List<ITUnitProject> tUnitProjects);
        Task<List<ITUnitProject>> GetTUnitProjectsAsync();
        void Remove(IVsHierarchy project);
        void Add(ITUnitProject iTUnitProject);
        void Clear();
    }
}
