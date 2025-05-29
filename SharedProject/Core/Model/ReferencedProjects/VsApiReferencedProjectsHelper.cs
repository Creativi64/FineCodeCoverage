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
        private readonly ICPPReferencedProjectsHelper cppReferencedProjectsHelper;
        private readonly IDotNetReferencedProjectsHelper dotNetReferencedProjectsHelper;
        private readonly AsyncLazy<DTE2> lazyDTE2;

        [ImportingConstructor]
        public VsApiReferencedProjectsHelper(
            [Import(typeof(SVsServiceProvider))]
            IServiceProvider serviceProvider,
            ICPPReferencedProjectsHelper cppReferencedProjectsHelper,
            IDotNetReferencedProjectsHelper dotNetReferencedProjectsHelper
        )
        {
            this.lazyDTE2 = new AsyncLazy<DTE2>(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                return (DTE2)serviceProvider.GetService(typeof(DTE));
            }, ThreadHelper.JoinableTaskFactory);
            this.cppReferencedProjectsHelper = cppReferencedProjectsHelper;
            this.dotNetReferencedProjectsHelper = dotNetReferencedProjectsHelper;
        }
        public async Task<List<IExcludableReferencedProject>> GetReferencedProjectsAsync(string projectFile)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            Project project = await this.GetProjectAsync(projectFile);

            if (project == null)
            {
                return null;
            }

            if (project.Object is VCProject cppProject)
            {
                return await this.cppReferencedProjectsHelper.GetInstrumentableReferencedProjectsAsync(cppProject);
            }

            if (project.Object is VSProject vsProject)
            {
                return await this.dotNetReferencedProjectsHelper.GetReferencedProjectsAsync(vsProject);
            }

            return null;
        }

        private async Task<Project> GetProjectAsync(string projectFile)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            DTE2 dte2 = await this.lazyDTE2.GetValueAsync();
            // note that cannot do dte.Solution.Projects.Item(ProjectFile) - fails when dots in path
            return dte2.Solution.Projects.Cast<Project>().FirstOrDefault(p =>
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                //have to try here as unloaded projects will throw
                string projectFullName = "";
                try
                {
                    projectFullName = p.FullName;
                }
                catch { }

                return projectFullName == projectFile;
            });
        }
    }

}
