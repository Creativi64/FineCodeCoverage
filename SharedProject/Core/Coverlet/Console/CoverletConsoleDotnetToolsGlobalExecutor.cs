using System.ComponentModel.Composition;
using System.Threading.Tasks;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Output;

namespace FineCodeCoverage.Engine.Coverlet
{
    internal interface ICoverletConsoleDotnetToolsGlobalExecutor : ICoverletConsoleExecutor
    {
    }

    [Export(typeof(ICoverletConsoleDotnetToolsGlobalExecutor))]
    internal class CoverletConsoleDotnetToolsGlobalExecutor : ICoverletConsoleDotnetToolsGlobalExecutor
    {
        private readonly IDotNetToolListCoverlet dotNetToolListCoverlet;
        private readonly ILogger logger;

        [ImportingConstructor]
        public CoverletConsoleDotnetToolsGlobalExecutor(IDotNetToolListCoverlet dotNetToolListCoverlet, ILogger logger)
        {
            this.dotNetToolListCoverlet = dotNetToolListCoverlet;
            this.logger = logger;
        }
        public async Task<ExecuteRequest> GetRequestAsync(ICoverageProject coverageProject, string coverletSettings)
        {
            if (coverageProject.Settings.CoverletConsoleGlobal)
            {
                var details = await dotNetToolListCoverlet.GlobalAsync();
                if (details == null)
                {
                    await logger.LogAsync("Unable to use Coverlet console global tool");
                    return null;
                }
                return new ExecuteRequest
                {
                    FilePath = details.Command,
                    Arguments = coverletSettings,
                    WorkingDirectory = coverageProject.ProjectOutputFolder
                };
            }
            return null;
        }
    }
}
