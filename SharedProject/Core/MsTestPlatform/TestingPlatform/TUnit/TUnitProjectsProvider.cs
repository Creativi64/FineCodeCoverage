using FineCodeCoverage.Core.Utilities;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio;
using NuGet.VisualStudio.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.ComponentModel.Composition;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    [Export(typeof(ITUnitProjectsProvider))]
    internal class TUnitProjectsProvider : ITUnitProjectsProvider
    {
        private readonly IServiceProvider serviceProvider;
        private readonly AsyncLazy<INuGetProjectService> lazyNugetProjectService;

        [ImportingConstructor]
        public TUnitProjectsProvider(
            [Import(typeof(SVsServiceProvider))]
            IServiceProvider serviceProvider,
            AsyncServiceProviderProvider asyncServiceProviderProvider
        )
        {
            this.serviceProvider = serviceProvider;
            lazyNugetProjectService = new AsyncLazy<INuGetProjectService>(async () =>
            {
                var brokeredServiceContainer = await asyncServiceProviderProvider.Provider.GetServiceAsync<SVsBrokeredServiceContainer, IBrokeredServiceContainer>();
                IServiceBroker serviceBroker = brokeredServiceContainer.GetFullAccessServiceBroker();
#pragma warning disable ISB001 // Dispose of proxies
                INuGetProjectService nugetProjectService = await serviceBroker.GetProxyAsync<INuGetProjectService>(NuGetServices.NuGetProjectServiceV1);
#pragma warning restore ISB001 // Dispose of proxies
                return nugetProjectService;
            }, ThreadHelper.JoinableTaskFactory);
        }
        public async Task<List<IVsHierarchy>> GetTUnitProjectsWithCoverageExtensionAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var vsSolution = serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
            var result = vsSolution.GetProjectEnum((uint)__VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION, Guid.Empty, out var enumHierarchies);
            if (result == VSConstants.S_OK)
            {
                return await GetApplicableProjectsAsync(enumHierarchies);
            }
            return Enumerable.Empty<IVsHierarchy>().ToList();
        }
        private async Task<List<IVsHierarchy>> GetApplicableProjectsAsync(IEnumHierarchies vsEnumHierarchies)
        {
            List<IVsHierarchy> applicableProjects = new List<IVsHierarchy>();
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsHierarchy[] rgelt = new IVsHierarchy[1];
            uint fetched = 0;
            while (vsEnumHierarchies.Next(1, rgelt, out fetched) == VSConstants.S_OK && fetched > 0)
            {
                IVsHierarchy projectHierarchy = rgelt[0];
                if (await IsApplicableProjectAsync(projectHierarchy))
                {
                    applicableProjects.Add(projectHierarchy);
                }
            }
            return applicableProjects;
        }

        private async Task<bool> IsApplicableProjectAsync(IVsHierarchy project)
        {
            /*
                could have used ?
                from TUnit.Engine.props
                <IsTestProject>true</IsTestProject>
            */
            return project.IsCapabilityMatch("TestContainer") && await IsTUnitWithCoverageExtensionAsync(project);
        }

        private async Task<bool> IsTUnitWithCoverageExtensionAsync(IVsHierarchy vsHierarchy)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var isTUnitWithCoverageExtension = false;
            var nugetProjectService = await lazyNugetProjectService.GetValueAsync();
            // might throw ?
            var installedPackagesResult = await nugetProjectService.GetInstalledPackagesAsync(vsHierarchy.GetGuid(), CancellationToken.None);
            if (installedPackagesResult.Status == InstalledPackageResultStatus.Successful)
            {
                var hasTUnit = false;
                var hasCoverageExtension = false;
                foreach (var package in installedPackagesResult.Packages)
                {
                    var id = package.Id;
                    if (id == "TUnit")
                    {
                        hasTUnit = true;
                        continue;
                    }
                    if (id == "Microsoft.Testing.Extensions.CodeCoverage")
                    {
                        hasCoverageExtension = true;
                    }
                }
                isTUnitWithCoverageExtension = hasTUnit && hasCoverageExtension;
            }
            return isTUnitWithCoverageExtension;
        }
    }

}
