using System;
using System.ComponentModel.Composition;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace FineCodeCoverage.Core.MsTestPlatform.CodeCoverage
{
    [Export(typeof(IProjectSaver))]
    internal sealed class ProjectSaver : IProjectSaver
    {
        private readonly IServiceProvider _serviceProvider;

        [ImportingConstructor]
        public ProjectSaver(
            [Import(typeof(SVsServiceProvider))]
            IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        public async System.Threading.Tasks.Task SaveProjectAsync(IVsHierarchy projectHierarchy)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var _solution = (IVsSolution)_serviceProvider.GetService(typeof(SVsSolution));
            Assumes.Present(_solution);
            int hr = _solution.SaveSolutionElement((uint)__VSSLNSAVEOPTIONS.SLNSAVEOPT_SaveIfDirty, projectHierarchy, 0);
            if (ErrorHandler.Failed(hr))
            {
            }
        }
    }
}
