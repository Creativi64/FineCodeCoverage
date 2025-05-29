using System.ComponentModel.Composition;
using System.Threading.Tasks;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Output;

namespace FineCodeCoverage.Engine.Coverlet
{
    [Export(typeof(ICoverletConsoleDotnetToolsLocalExecutor))]
    internal class CoverletConsoleDotnetToolsLocalExecutor : ICoverletConsoleDotnetToolsLocalExecutor
    {
        private readonly IDotNetToolListCoverlet dotnetToolListCoverlet;
        private readonly IDotNetConfigFinder dotNetConfigFinder;
        private readonly ILogger logger;

        [ImportingConstructor]
        public CoverletConsoleDotnetToolsLocalExecutor(IDotNetToolListCoverlet dotnetToolListCoverlet, IDotNetConfigFinder dotNetConfigFinder, ILogger logger)
        {
            this.dotnetToolListCoverlet = dotnetToolListCoverlet;
            this.dotNetConfigFinder = dotNetConfigFinder;
            this.logger = logger;
        }
        public async Task<ExecuteRequest> GetRequestAsync(ICoverageProject coverageProject, string coverletSettings)
        {
            if (coverageProject.Settings.CoverletConsoleLocal)
            {
                foreach (string configContainingDirectory in dotNetConfigFinder.GetConfigDirectories(coverageProject.ProjectOutputFolder))
                {
                    CoverletDotNetToolDetails coverletToolDetails = await dotnetToolListCoverlet.LocalAsync(configContainingDirectory);
                    if (coverletToolDetails != null)
                    {
                        return new ExecuteRequest
                        {
                            FilePath = "dotnet",
                            Arguments = coverletToolDetails.Command + " " + coverletSettings,
                            WorkingDirectory = configContainingDirectory
                        };
                    }
                }

                await this.logger.LogAsync("Unable to use Coverlet console local tool");
            }
            return null;
        }
    }
}
