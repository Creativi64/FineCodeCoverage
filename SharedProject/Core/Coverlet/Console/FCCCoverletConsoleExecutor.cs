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
        private const string ZipPrefix = "coverlet.console";
        private const string ZipDirectoryName = "coverlet"; // backwards compatibility
        private readonly IToolUnzipper _toolUnzipper;
        private string _coverletExePath;

        [ImportingConstructor]
        public FCCCoverletConsoleExecutor(IToolUnzipper toolUnzipper) => _toolUnzipper = toolUnzipper;

        public Task<ExecuteRequest> GetRequestAsync(ICoverageProject coverageProject, string coverletSettings)
            => Task.FromResult(
                new ExecuteRequest
                {
                    FilePath = _coverletExePath,
                    Arguments = coverletSettings,
                    WorkingDirectory = coverageProject.ProjectOutputFolder
                }
            );

        public void Initialize(string appDataFolder, CancellationToken cancellationToken)
        {
            string zipDestination = _toolUnzipper.EnsureUnzipped(appDataFolder, ZipDirectoryName, ZipPrefix, cancellationToken);
            _coverletExePath = Directory.GetFiles(zipDestination, "coverlet.exe", SearchOption.AllDirectories).FirstOrDefault()
                           ?? Directory.GetFiles(zipDestination, "*coverlet*.exe", SearchOption.AllDirectories).FirstOrDefault();
        }
    }
}
