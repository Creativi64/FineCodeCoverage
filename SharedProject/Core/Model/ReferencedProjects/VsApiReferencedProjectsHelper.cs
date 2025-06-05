using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.VCProjectEngine;
using VSLangProj;

namespace FineCodeCoverage.Engine.Model
{
    [Export(typeof(IVsApiReferencedProjectsHelper))]
    internal class VsApiReferencedProjectsHelper : IVsApiReferencedProjectsHelper
    {
        private readonly ICPPReferencedProjectsHelper _cppReferencedProjectsHelper;
        private readonly IDotNetReferencedProjectsHelper _dotNetReferencedProjectsHelper;
        private readonly AsyncLazy<DTE2> _lazyDTE2;

        [ImportingConstructor]
        public VsApiReferencedProjectsHelper(
            [Import(typeof(SVsServiceProvider))]
            IServiceProvider serviceProvider,
            ICPPReferencedProjectsHelper cppReferencedProjectsHelper,
            IDotNetReferencedProjectsHelper dotNetReferencedProjectsHelper
        )
        {
            _lazyDTE2 = new AsyncLazy<DTE2>(
                async () =>
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    return (DTE2)serviceProvider.GetService(typeof(DTE));
                }, ThreadHelper.JoinableTaskFactory
            );
            _cppReferencedProjectsHelper = cppReferencedProjectsHelper;
            _dotNetReferencedProjectsHelper = dotNetReferencedProjectsHelper;
        }

        public async Task<List<IExcludableReferencedProject>> GetReferencedProjectsAsync(string projectFile)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            Project project = await GetProjectAsync(projectFile);

            return project == null
                ? null
                : project.Object is VCProject cppProject
                ? await _cppReferencedProjectsHelper.GetInstrumentableReferencedProjectsAsync(cppProject)
                : project.Object is VSProject vsProject ?
                await _dotNetReferencedProjectsHelper.GetReferencedProjectsAsync(vsProject)
                : null;
        }

        private async Task<Project> GetProjectAsync(string projectFile)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            DTE2 dte2 = await _lazyDTE2.GetValueAsync();

            // note that cannot do dte.Solution.Projects.Item(ProjectFile) - fails when dots in path
            return dte2.Solution.Projects.Cast<Project>().FirstOrDefault(p =>
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                // have to try here as unloaded projects will throw
                string projectFullName = string.Empty;
                try
                {
                    projectFullName = p.FullName;
                }
                catch
                {
                }

                return projectFullName == projectFile;
            });
        }
    }
}
