using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.Shell.Interop;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    internal interface ICPSTestProjectService
    {
        Task<ConfiguredProject> GetProjectAsync(IVsHierarchy hierarchy);
    }
}