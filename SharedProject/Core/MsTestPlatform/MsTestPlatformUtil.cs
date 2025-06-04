using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Core.Initialization;
using FineCodeCoverage.Core.Utilities;

namespace FineCodeCoverage.Engine.MsTestPlatform
{
    [Export(typeof(IMsTestPlatformUtil))]
    [Export(typeof(IAppDataFolderPathDependent))]
    internal class MsTestPlatformUtil : IMsTestPlatformUtil, IAppDataFolderPathDependent
    {
        public string MsTestPlatformExePath { get; private set; }
        private readonly IToolUnzipper _toolUnzipper;
        private const string ZipPrefix = "microsoft.testplatform";
        private const string ZipDirectoryName = "msTestPlatform";

        [ImportingConstructor]
        public MsTestPlatformUtil(IToolUnzipper toolUnzipper) => _toolUnzipper = toolUnzipper;

        public Task InitializeAsync(string appDataFolderPath, CancellationToken cancellationToken)
        {
            string zipDestination = _toolUnzipper.EnsureUnzipped(appDataFolderPath, ZipDirectoryName, ZipPrefix, cancellationToken);
            MsTestPlatformExePath = Directory
                .GetFiles(zipDestination, "vstest.console.exe", SearchOption.AllDirectories)
                .FirstOrDefault();
            return Task.CompletedTask;
        }
    }
}
