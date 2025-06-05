using System.ComponentModel.Composition;
using System.Threading.Tasks;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Output;

namespace FineCodeCoverage.Engine.Coverlet
{
    [Export(typeof(ICoverletConsoleDotnetToolsGlobalExecutor))]
    internal class CoverletConsoleDotnetToolsGlobalExecutor : ICoverletConsoleDotnetToolsGlobalExecutor
    {
        private readonly IDotNetToolListCoverlet _dotNetToolListCoverlet;
        private readonly ILogger _logger;

        [ImportingConstructor]
        public CoverletConsoleDotnetToolsGlobalExecutor(
            IDotNetToolListCoverlet dotNetToolListCoverlet,
            ILogger logger)
        {
            _dotNetToolListCoverlet = dotNetToolListCoverlet;
            _logger = logger;
        }

        public async Task<ExecuteRequest> GetRequestAsync(ICoverageProject coverageProject, string coverletSettings)
        {
            if (!coverageProject.Settings.CoverletConsoleGlobal)
            {
                return null;
            }

            CoverletDotNetToolDetails details = await _dotNetToolListCoverlet.GlobalAsync();
            if (details == null)
            {
                await _logger.LogAsync("Unable to use Coverlet console global tool");
                return null;
            }

            return new ExecuteRequest
            {
                FilePath = details.Command,
                Arguments = coverletSettings,
                WorkingDirectory = coverageProject.ProjectOutputFolder,
            };
        }
    }
}
