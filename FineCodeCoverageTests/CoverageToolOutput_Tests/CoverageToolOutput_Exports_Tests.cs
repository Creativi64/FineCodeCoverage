using FineCodeCoverage.Collection.CoverageToolOutput;
using FineCodeCoverageTests.TestHelpers;
using NUnit.Framework;

namespace FineCodeCoverageTests
{
    class CoverageToolOutput_Exports_Tests
    {
        [Test]
        public void ICoverageToolOutputFolderProvider_Should_Have_Consistent_Ordered_Exports()
        {
            MefOrderAssertions.InterfaceExportsHaveConsistentOrder(typeof(ICoverageToolOutputFolderProvider));
        }
    }
}
