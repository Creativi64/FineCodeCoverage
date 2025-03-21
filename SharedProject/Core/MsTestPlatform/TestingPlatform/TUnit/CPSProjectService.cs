using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    [Export(typeof(ICPSProjectService))]
    internal class CPSProjectService : ICPSProjectService
    {
        public async Task<ConfiguredProject> GetProjectAsync(IVsHierarchy hierarchy)
        {
            var unconfiguredProject = await hierarchy.AsUnconfiguredProjectAsync();
            if (unconfiguredProject == null) return null;
            return await unconfiguredProject.GetSuggestedConfiguredProjectAsync();
        }

        public ConfiguredProject GetProject(IVsHierarchy hierarchy)
        {
#pragma warning disable VSTHRD102 // Implement internal logic asynchronously
            return ThreadHelper.JoinableTaskFactory.Run(() => GetProjectAsync(hierarchy));
#pragma warning restore VSTHRD102 // Implement internal logic asynchronously
        }
    }
}
