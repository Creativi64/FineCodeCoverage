using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    [Export(typeof(ITestProjectsProvider))]
    internal class TestProjectsProvider : ITestProjectsProvider
    {
        private readonly ISolutionProjectsProvider solutionProjectsProvider;

        [ImportingConstructor]
        public TestProjectsProvider(
            ISolutionProjectsProvider solutionProjectsProvider
        )
        {
            this.solutionProjectsProvider = solutionProjectsProvider;
        }

        public bool IsTestProject(IVsHierarchy project)
        {
            return project.IsCapabilityMatch("TestContainer");
        }

        public async Task<IEnumerable<IVsHierarchy>> ProvideAsync()
        {
            var projects = await solutionProjectsProvider.GetProjectsAsync();
            return projects.Where(IsTestProject);
        }
    }
}
