using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell.Interop;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    internal interface ITUnitProjectCache
    {
        void Initialize(List<ITUnitProject> tUnitProjects);

        Task<List<ITUnitProject>> GetTUnitProjectsAsync(CancellationToken cancellationToken);

        void Remove(IVsHierarchy project);

        void Add(ITUnitProject iTUnitProject);

        void Clear();
    }
}
