using System;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Threading;
using NuGet.VisualStudio.Contracts;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{

    [Export(typeof(ITUnitInstalledPackagesService))]
    internal class TUnitInstalledPackagesService : ITUnitInstalledPackagesService
    {
        private readonly AsyncLazy<INuGetProjectService> _lazyNugetProjectService;

        [ImportingConstructor]
        public TUnitInstalledPackagesService(
            INugetProjectServiceProvider nugetProjectServiceProvider
        ) => _lazyNugetProjectService = nugetProjectServiceProvider.LazyNugetProjectService;

        public TUnitInstalledPackageResult GetTUnitInstalledPackages(IImmutableDictionary<string, IImmutableDictionary<string, string>> packageReferenceItems)
        {
            if (packageReferenceItems == null)
            {
                return new TUnitInstalledPackageResult(InstalledPackageResultStatus.Unknown, false, false);
            }

            bool hasTUnit = false;
            bool hasCoverageExtension = false;
            foreach (System.Collections.Generic.KeyValuePair<string, IImmutableDictionary<string, string>> packageReference in packageReferenceItems)
            {
                string id = packageReference.Key;
                if (id == TUnitConstants.TUnitPackageId)
                {
                    hasTUnit = true;
                    continue;
                }

                if (id == TUnitConstants.CodeCoveragePackageId)
                {
                    hasCoverageExtension = true;
                }

                if (hasTUnit && hasCoverageExtension)
                {
                    break;
                }
            }

            return new TUnitInstalledPackageResult(InstalledPackageResultStatus.Successful, hasCoverageExtension, hasTUnit);
        }

        public async Task<TUnitInstalledPackageResult> GetTUnitInstalledPackagesAsync(Guid projectGuid, CancellationToken cancellationToken)
        {
            INuGetProjectService nugetProjectService = await _lazyNugetProjectService.GetValueAsync();
            InstalledPackagesResult result = await nugetProjectService.GetInstalledPackagesAsync(projectGuid, cancellationToken);
            if (result.Status == InstalledPackageResultStatus.Successful)
            {
                bool hasTUnit = false;
                bool hasCoverageExtension = false;
                foreach (NuGetInstalledPackage package in result.Packages)
                {
                    string id = package.Id;
                    if (id == TUnitConstants.TUnitPackageId)
                    {
                        hasTUnit = true;
                        continue;
                    }

                    if (id == TUnitConstants.CodeCoveragePackageId)
                    {
                        hasCoverageExtension = true;
                    }

                    if (hasTUnit && hasCoverageExtension)
                    {
                        break;
                    }
                }

                return new TUnitInstalledPackageResult(result.Status, hasCoverageExtension, hasTUnit);
            }

            return new TUnitInstalledPackageResult(result.Status, false, false);
        }
    }
}
