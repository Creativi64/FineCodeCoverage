using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

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
        ) => this.serviceProvider = serviceProvider;

        public async Task<List<IVsHierarchy>> GetLoadedProjectsAsync(CancellationToken cancellationToken)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            var vsSolution = this.serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
            return this.GetProjects(vsSolution, __VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION);
        }

        public async Task<bool> IsSolutionOpenAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var vsSolution = this.serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
            Assumes.Present(vsSolution);
            _ = vsSolution.GetProperty((int)__VSPROPID.VSPROPID_IsSolutionOpen, out object isSolutionOpen);
            return (bool)isSolutionOpen;
        }

        private List<IVsHierarchy> GetProjects(IVsSolution vsSolution, __VSENUMPROJFLAGS flags)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var projects = new List<IVsHierarchy>();
            int result = vsSolution.GetProjectEnum((uint)flags, Guid.Empty, out IEnumHierarchies enumHierarchies);
            if (result == VSConstants.S_OK)
            {
                var rgelt = new IVsHierarchy[1];
                while (enumHierarchies.Next(1, rgelt, out uint fetched) == VSConstants.S_OK && fetched > 0)
                {
                    _ = rgelt[0].GetGuidProperty(
                        VSConstants.VSITEMID_ROOT,
                        (int)__VSHPROPID.VSHPROPID_TypeGuid,
                        out Guid typeGuid
                    );

                    if (typeGuid != VSConstants.GUID_ItemType_VirtualFolder)
                    {
                        projects.Add(rgelt[0]);
                    }
                }
            }

            return projects;
        }
    }
}
