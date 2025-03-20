using Microsoft.VisualStudio.Shell.Interop;
using NuGet.VisualStudio.Contracts;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    [Export(typeof(ITUnitProjectFactory))]
    internal class TUnitProjectFactory : ITUnitProjectFactory
    {
        private readonly ITUnitInstalledPackagesService tUnitInstalledPackagesService;

        class TUnitProject : ITUnitProject
        {
            private readonly ITUnitInstalledPackagesService tUnitInstalledPackagesService;
            private bool requiresUpdate = true;
            public TUnitProject(ITUnitInstalledPackagesService tUnitInstalledPackagesService, IVsHierarchy hierarchy)
            {
                Hierarchy = hierarchy;
                this.tUnitInstalledPackagesService = tUnitInstalledPackagesService;
            }
            public bool IsTUnit { get; private set; }
            public bool HasCoverageExtension { get; private set; }
            public IVsHierarchy Hierarchy { get; }

            public async Task UpdateStateAsync(bool force)
            {
                if (requiresUpdate || force)
                {
                    var installedPackagesResult = await tUnitInstalledPackagesService.GetTUnitInstalledPackagesAsync(await Hierarchy.GetGuidAsync(), CancellationToken.None);
                    if (installedPackagesResult.Status == InstalledPackageResultStatus.Successful)
                    {
                        IsTUnit = installedPackagesResult.HasTUnit;
                        HasCoverageExtension = installedPackagesResult.HasCoverageExtension;
                    }
                    else
                    {
                        //todo
                    }
                    requiresUpdate = false;
                }
            }
        }


        [ImportingConstructor]
        public TUnitProjectFactory(
            ITUnitInstalledPackagesService tUnitInstalledPackagesService
        )
        {
            this.tUnitInstalledPackagesService = tUnitInstalledPackagesService;
        }
        public ITUnitProject Create(IVsHierarchy project)
        {
            return new TUnitProject(tUnitInstalledPackagesService, project);
        }
    }
}
