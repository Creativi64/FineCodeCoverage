using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace FineCodeCoverage.Collection.TestingPlatform.TUnit
{
    [Export(typeof(ICPSTestProjectService))]
    internal sealed class CPSTestProjectService : ICPSTestProjectService
    {
        public async Task<ConfiguredProject> GetProjectAsync(IVsHierarchy hierarchy)
        {
            if (!hierarchy.IsCapabilityMatch("TestContainer"))
            {
                return null;
            }

            UnconfiguredProject unconfiguredProject = await hierarchy.AsUnconfiguredProjectAsync();
            return unconfiguredProject == null ? null : await unconfiguredProject.GetSuggestedConfiguredProjectAsync();
        }
    }
}
