using NuGet.VisualStudio.Contracts;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    internal sealed class TUnitInstalledPackageResult
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
}
