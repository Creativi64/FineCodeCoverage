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
        private readonly IVsApiReferencedProjectsHelper _vsApiReferencedProjectsHelper;
        private readonly IProjectFileReferencedProjectsHelper _projectFileReferencedProjectsHelper;
        private string _projectFile;
        private Func<XElement> _projectFileXElementProvider;

        [ImportingConstructor]
        public ReferencedProjectsHelper(
            IVsApiReferencedProjectsHelper vsApiReferencedProjectsHelper,
            IProjectFileReferencedProjectsHelper projectFileReferencedProjectsHelper
        )
        {
            _vsApiReferencedProjectsHelper = vsApiReferencedProjectsHelper;
            _projectFileReferencedProjectsHelper = projectFileReferencedProjectsHelper;
        }

        public async Task<List<IExcludableReferencedProject>> GetReferencedProjectsAsync(
            string projectFile,
            Func<XElement> projectFileXElementProvider
        )
        {
            _projectFileXElementProvider = projectFileXElementProvider;
            _projectFile = projectFile;
            List<IExcludableReferencedProject> referencedProjects = await GetReferencedProjectsAsync();
            return new List<IExcludableReferencedProject>(referencedProjects);
        }

        private async Task<List<IExcludableReferencedProject>> GetReferencedProjectsAsync()
            => await SafeGetReferencedProjectsFromVSApiAsync() ??
            await _projectFileReferencedProjectsHelper.GetReferencedProjectsAsync(
                _projectFile, _projectFileXElementProvider());

        private async Task<List<IExcludableReferencedProject>> SafeGetReferencedProjectsFromVSApiAsync()
        {
            try
            {
                return await _vsApiReferencedProjectsHelper.GetReferencedProjectsAsync(_projectFile);
            }
            catch
            {
            }

            return null;
        }
    }
}
