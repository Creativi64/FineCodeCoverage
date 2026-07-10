using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverage.Collection.TestingPlatform.TUnit
{
    internal interface ITUnitInstalledPackagesService
    {
        TUnitInstalledPackageResult GetTUnitInstalledPackages(IImmutableDictionary<string, IImmutableDictionary<string, string>> packageReferenceItems);

        Task<TUnitInstalledPackageResult> GetTUnitInstalledPackagesAsync(Guid projectGuid, CancellationToken cancellationToken);
    }
}
