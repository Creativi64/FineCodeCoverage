using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine.Model;

namespace FineCodeCoverage.Engine.Coverlet
{
    [Export(typeof(IFCCCoverletConsoleExecutor))]
    internal class FCCCoverletConsoleExecutor : IFCCCoverletConsoleExecutor
    {
        [ImportingConstructor]
        public FCCCoverletConsoleExecutor(IToolUnzipper toolUnzipper) => this.toolUnzipper = toolUnzipper;

        private string coverletExePath;
        private const string zipPrefix = "coverlet.console";
        private const string zipDirectoryName = "coverlet";//backwards compatibility
        private readonly IToolUnzipper toolUnzipper;

        public Task<ExecuteRequest> GetRequestAsync(ICoverageProject coverageProject, string coverletSettings)
            => Task.FromResult(
                new ExecuteRequest
                {
                    FilePath = this.coverletExePath,
                    Arguments = coverletSettings,
                    WorkingDirectory = coverageProject.ProjectOutputFolder
                }
            );

        public void Initialize(string appDataFolder, CancellationToken cancellationToken)
        {
            string zipDestination = this.toolUnzipper.EnsureUnzipped(appDataFolder, zipDirectoryName, zipPrefix, cancellationToken);
            this.coverletExePath = Directory.GetFiles(zipDestination, "coverlet.exe", SearchOption.AllDirectories).FirstOrDefault()
                           ?? Directory.GetFiles(zipDestination, "*coverlet*.exe", SearchOption.AllDirectories).FirstOrDefault();
        }
    }
}
