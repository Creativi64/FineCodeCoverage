using FineCodeCoverage.Core.Initialization;
using FineCodeCoverage.Core.MsTestPlatform.TestingPlatform;
using FineCodeCoverageTests.TestHelpers;
using NUnit.Framework;

namespace Test
{
    public class TUnitRunner_Tests
    {
        [Test]
        public void Should_Be_IAppDataFolderPathDependent()
        {
            Assert.That(MEFExportHelper.IsAndExports<TUnitCoverageRunner, IAppDataFolderPathDependent>(), Is.True);
        }
	}
}