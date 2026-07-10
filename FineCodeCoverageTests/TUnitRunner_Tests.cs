using FineCodeCoverage.Collection.TestingPlatform.TUnit;
using FineCodeCoverage.Initialization;
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