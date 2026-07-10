using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.Shell.Interop;

namespace FineCodeCoverage.Collection.TestingPlatform.TUnit
{
    internal interface ICPSTestProjectService
    {
        Task<ConfiguredProject> GetProjectAsync(IVsHierarchy hierarchy);
    }
}
