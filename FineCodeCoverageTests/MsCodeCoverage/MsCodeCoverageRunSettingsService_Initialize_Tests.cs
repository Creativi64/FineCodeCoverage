using NUnit.Framework;
using AutoMoq;
using FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverageTests.TestHelpers;
using FineCodeCoverage.Initialization;
using FineCodeCoverage.Initialization.ToolZip;

namespace FineCodeCoverageTests.MsCodeCoverage
{
    internal class MsCodeCoverageRunSettingsService_Initialize_Tests
    {
        [Test]
        public void Should_Be_IAppDataFolderPathDependent()
        {
            Assert.That(MEFExportHelper.IsAndExports<MsCodeCoverageRunSettingsService, IAppDataFolderPathDependent>(), Is.True);
        }

        [Test]
        public async Task Should_Ensure_Microsoft_CodeCoverage_Is_Unzipped_To_The_Tool_Folder_Async()
        {
            var autoMocker = new AutoMoqer();
            var msCodeCoverageRunSettingsService  = autoMocker.Create<MsCodeCoverageRunSettingsService>();

            var cancellationToken = CancellationToken.None;

            var mockToolUnzipper = autoMocker.GetMock<IToolUnzipper>();
            mockToolUnzipper.Setup(toolFolder => 
                toolFolder.EnsureUnzipped("AppDataFolder", "msCodeCoverage", "microsoft.codecoverage", cancellationToken)).Returns("ZipDestination");
            
            await msCodeCoverageRunSettingsService.InitializeAsync("AppDataFolder", cancellationToken);
            mockToolUnzipper.VerifyAll();
        }
    }
}
