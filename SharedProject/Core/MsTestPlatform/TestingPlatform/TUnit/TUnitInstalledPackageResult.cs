using NuGet.VisualStudio.Contracts;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    internal class TUnitInstalledPackageResult
    {
        public TUnitInstalledPackageResult(InstalledPackageResultStatus status, bool hasCoverageExtension, bool hasTunit)
        {
            this.Status = status;
            this.HasCoverageExtension = hasCoverageExtension;
            this.HasTUnit = hasTunit;
        }

        public bool HasTUnit { get; }
        public bool HasCoverageExtension { get; }
        public InstalledPackageResultStatus Status { get; }
    }
}