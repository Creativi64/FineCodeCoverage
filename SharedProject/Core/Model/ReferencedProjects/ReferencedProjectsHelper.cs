using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FineCodeCoverage.Engine.Model
{
    [Export(typeof(IReferencedProjectsHelper))]
    internal class ReferencedProjectsHelper : IReferencedProjectsHelper
    {
        private readonly IVsApiReferencedProjectsHelper vsApiReferencedProjectsHelper;
        private readonly IProjectFileReferencedProjectsHelper projectFileReferencedProjectsHelper;
        private string projectFile;
        private Func<XElement> projectFileXElementProvider;

        [ImportingConstructor]
        public ReferencedProjectsHelper(
            IVsApiReferencedProjectsHelper vsApiReferencedProjectsHelper,
            IProjectFileReferencedProjectsHelper projectFileReferencedProjectsHelper
        )
        {
            this.vsApiReferencedProjectsHelper = vsApiReferencedProjectsHelper;
            this.projectFileReferencedProjectsHelper = projectFileReferencedProjectsHelper;
        }

        public async Task<List<IExcludableReferencedProject>> GetReferencedProjectsAsync(string projectFile, Func<XElement> projectFileXElementProvider)
        {
            this.projectFileXElementProvider = projectFileXElementProvider;
            this.projectFile = projectFile;
            List<IExcludableReferencedProject> referencedProjects = await GetReferencedProjectsAsync();
            return new List<IExcludableReferencedProject>(referencedProjects);
        }

        private async Task<List<IExcludableReferencedProject>> GetReferencedProjectsAsync()
        {
            return await SafeGetReferencedProjectsFromVSApiAsync() ?? await projectFileReferencedProjectsHelper.GetReferencedProjectsAsync(projectFile, projectFileXElementProvider());
        }

        private async Task<List<IExcludableReferencedProject>> SafeGetReferencedProjectsFromVSApiAsync()
        {
            try
            {
                return await vsApiReferencedProjectsHelper.GetReferencedProjectsAsync(projectFile);
            }
            catch { }
            return null;
        }
    }

}
