using NuGet.VisualStudio.Contracts;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    internal class TUnitInstalledPackageResult
    {
        public TUnitInstalledPackageResult(InstalledPackageResultStatus status, bool hasCoverageExtension, bool hasTunit)
        {
            Status = status;
            HasCoverageExtension = hasCoverageExtension;
            HasTUnit = hasTunit;
        }

        public bool HasTUnit { get; }
        public bool HasCoverageExtension { get; }
        public InstalledPackageResultStatus Status { get; }
    }

    interface ITUnitInstalledPackagesService
    {
        Task<TUnitInstalledPackageResult> GetTUnitInstalledPackagesAsync(Guid projectGuid, CancellationToken cancellationToken);
    }
}
