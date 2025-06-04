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
            this._vsApiReferencedProjectsHelper = vsApiReferencedProjectsHelper;
            this._projectFileReferencedProjectsHelper = projectFileReferencedProjectsHelper;
        }

        public async Task<List<IExcludableReferencedProject>> GetReferencedProjectsAsync(
            string projectFile,
            Func<XElement> projectFileXElementProvider
        )
        {
            this._projectFileXElementProvider = projectFileXElementProvider;
            this._projectFile = projectFile;
            List<IExcludableReferencedProject> referencedProjects = await this.GetReferencedProjectsAsync();
            return new List<IExcludableReferencedProject>(referencedProjects);
        }

        private async Task<List<IExcludableReferencedProject>> GetReferencedProjectsAsync()
            => await this.SafeGetReferencedProjectsFromVSApiAsync() ??
            await this._projectFileReferencedProjectsHelper.GetReferencedProjectsAsync(
                this._projectFile, this._projectFileXElementProvider());

        private async Task<List<IExcludableReferencedProject>> SafeGetReferencedProjectsFromVSApiAsync()
        {
            try
            {
                return await this._vsApiReferencedProjectsHelper.GetReferencedProjectsAsync(this._projectFile);
            }
            catch { }

            return null;
        }
    }
}