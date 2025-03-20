using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{

    [Export(typeof(ISolutionProjectsProvider))]
    internal class SolutionProjectsProvider : ISolutionProjectsProvider
    {
        private readonly IServiceProvider serviceProvider;

        [ImportingConstructor]
        public SolutionProjectsProvider(
            [Import(typeof(SVsServiceProvider))]
            IServiceProvider serviceProvider
        )
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task<List<IVsHierarchy>> GetProjectsAsync()
        {
            var projects = new List<IVsHierarchy>();
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var vsSolution = serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
            var result = vsSolution.GetProjectEnum((uint)__VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION, Guid.Empty, out var enumHierarchies);
            if (result == VSConstants.S_OK)
            {
                IVsHierarchy[] rgelt = new IVsHierarchy[1];
                uint fetched = 0;
                while (enumHierarchies.Next(1, rgelt, out fetched) == VSConstants.S_OK && fetched > 0)
                {
                    projects.Add(rgelt[0]);
                }
            }
            return projects;
        }
    }

}
