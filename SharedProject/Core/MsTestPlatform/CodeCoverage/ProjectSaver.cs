using System;
using System.ComponentModel.Composition;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace FineCodeCoverage.Core.MsTestPlatform.CodeCoverage
{
    [Export(typeof(IProjectSaver))]
    internal class ProjectSaver : IProjectSaver
    {
        private readonly IServiceProvider serviceProvider;

        [ImportingConstructor]
        public ProjectSaver(
            [Import(typeof(SVsServiceProvider))]
            IServiceProvider serviceProvider
        )
        {
            this.serviceProvider = serviceProvider;
        }

        public async System.Threading.Tasks.Task SaveProjectAsync(IVsHierarchy projectHierarchy)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var _solution = (IVsSolution)this.serviceProvider.GetService(typeof(SVsSolution));
            Assumes.Present(_solution);
            int hr = _solution.SaveSolutionElement((uint)__VSSLNSAVEOPTIONS.SLNSAVEOPT_SaveIfDirty, projectHierarchy, 0);
            if (ErrorHandler.Failed(hr))
            {
            }
        }
    }
}
