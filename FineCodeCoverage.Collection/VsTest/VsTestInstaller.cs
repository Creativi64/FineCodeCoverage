using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Initialization;
using FineCodeCoverage.Initialization.ToolZip;

namespace FineCodeCoverage.Collection.VsTest
{
    [Export(typeof(IVsTestInstaller))]
    [Export(typeof(IAppDataFolderPathDependent))]
    internal sealed class VsTestInstaller : IVsTestInstaller, IAppDataFolderPathDependent
    {
        private const string ZipPrefix = "microsoft.testplatform";
        private const string ZipDirectoryName = "msTestPlatform";
        private readonly IToolUnzipper _toolUnzipper;

        public string InstallPath { get; private set; }

        [ImportingConstructor]
        public VsTestInstaller(IToolUnzipper toolUnzipper) => _toolUnzipper = toolUnzipper;

        public Task InitializeAsync(string appDataFolderPath, CancellationToken cancellationToken)
        {
            string zipDestination = _toolUnzipper.EnsureUnzipped(appDataFolderPath, ZipDirectoryName, ZipPrefix, cancellationToken);
            InstallPath = Directory
                .GetFiles(zipDestination, "vstest.console.exe", SearchOption.AllDirectories)
                .FirstOrDefault();
            return Task.CompletedTask;
        }
    }
}
