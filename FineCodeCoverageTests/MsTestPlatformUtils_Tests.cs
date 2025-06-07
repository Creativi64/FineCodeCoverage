using FineCodeCoverage.Core.Initialization;
using FineCodeCoverage.Engine.MsTestPlatform;
using FineCodeCoverageTests.TestHelpers;
using NUnit.Framework;

namespace Test
{
    public class MsTestPlatformUtil_Tests
    {
        [Test]
        public void Should_Be_IAppDataFolderPathDependent()
        {
            Assert.That(MEFExportHelper.IsAndExports<VsTestInstaller, IAppDataFolderPathDependent>(), Is.True);
        }

	}
}