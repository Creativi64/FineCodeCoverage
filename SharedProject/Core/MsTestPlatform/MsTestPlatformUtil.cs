using System.IO;
using System.Linq;
using System.ComponentModel.Composition;
using FineCodeCoverage.Core.Utilities;
using System.Threading;
using FineCodeCoverage.Core.Initialization;
using System.Threading.Tasks;

namespace FineCodeCoverage.Engine.MsTestPlatform
{
    [Export(typeof(IMsTestPlatformUtil))]
	[Export(typeof(IAppDataFolderPathDependent))]
	internal class MsTestPlatformUtil:IMsTestPlatformUtil, IAppDataFolderPathDependent
	{
		public string MsTestPlatformExePath { get; private set; }
        private readonly IToolUnzipper toolUnzipper;
        private const string zipPrefix = "microsoft.testplatform";
		private const string zipDirectoryName = "msTestPlatform";

		[ImportingConstructor]
		public MsTestPlatformUtil(IToolUnzipper toolUnzipper)
        {
            this.toolUnzipper = toolUnzipper;
        }

        public Task InitializeAsync(string appDataFolderPath, CancellationToken cancellationToken)
        {
            var zipDestination = toolUnzipper.EnsureUnzipped(appDataFolderPath, zipDirectoryName, zipPrefix, cancellationToken);
            MsTestPlatformExePath = Directory
                .GetFiles(zipDestination, "vstest.console.exe", SearchOption.AllDirectories)
                .FirstOrDefault();
            return Task.CompletedTask;
        }
    }
}
