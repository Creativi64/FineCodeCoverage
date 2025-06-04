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
        private readonly IDotNetToolListCoverlet _dotnetToolListCoverlet;
        private readonly IDotNetConfigFinder _dotNetConfigFinder;
        private readonly ILogger _logger;

        [ImportingConstructor]
        public CoverletConsoleDotnetToolsLocalExecutor(
            IDotNetToolListCoverlet dotnetToolListCoverlet,
            IDotNetConfigFinder dotNetConfigFinder,
            ILogger logger
        )
        {
            this._dotnetToolListCoverlet = dotnetToolListCoverlet;
            this._dotNetConfigFinder = dotNetConfigFinder;
            this._logger = logger;
        }
        public async Task<ExecuteRequest> GetRequestAsync(ICoverageProject coverageProject, string coverletSettings)
        {
            if (coverageProject.Settings.CoverletConsoleLocal)
            {
                foreach (string configContainingDirectory in this._dotNetConfigFinder.GetConfigDirectories(coverageProject.ProjectOutputFolder))
                {
                    CoverletDotNetToolDetails coverletToolDetails = await this._dotnetToolListCoverlet.LocalAsync(configContainingDirectory);
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

                await this._logger.LogAsync("Unable to use Coverlet console local tool");
            }

            return null;
        }
    }
}