using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Output;

namespace FineCodeCoverage.Engine.Coverlet
{
    [Export(typeof(IDotNetToolListCoverlet))]
    internal class DotNetToolListCoverlet : IDotNetToolListCoverlet
    {
        private const string CoverletPackageId = "coverlet.console";
        private readonly ILogger logger;
        private readonly IDotNetToolListExecutor executor;
        private readonly IDotNetToolListParser parser;

        [ImportingConstructor]
        public DotNetToolListCoverlet(ILogger logger, IDotNetToolListExecutor executor, IDotNetToolListParser parser)
        {
            this.logger = logger;
            this.executor = executor;
            this.parser = parser;
        }

        private async Task<CoverletToolDetails> ExecuteAndParseAsync(Func<IDotNetToolListExecutor, DotNetToolListExecutionResult> execution)
        {
            const string title = "Dotnet tool list Coverlet";
            var result = execution(executor);
            if (result.ExitCode != 0)
            {
                await logger.LogAsync($"{title} Error", result.Output);
                return null;
            }
            List<DotNetTool> tools = null;
            try
            {
                tools = parser.Parse(result.Output);
            }
            catch (Exception)
            {
                await logger.LogAsync($"{title} Error parsing", result.Output);
                return null;
            }

            var coverletConsoleTool = tools.FirstOrDefault(tool => tool.PackageId == CoverletPackageId);
            if (coverletConsoleTool == null)
            {
                return null;
            }

            return new CoverletToolDetails
            {
                Version = coverletConsoleTool.Version,
                Command = coverletConsoleTool.Commands
            };
        }

        public Task<CoverletToolDetails> GlobalAsync()
        {
            return ExecuteAndParseAsync(executor => executor.Global());
        }

        public Task<CoverletToolDetails> LocalAsync(string directory)
        {
            return ExecuteAndParseAsync(executor => executor.Local(directory));
        }

        public Task<CoverletToolDetails> GlobalToolsPathAsync(string directory)
        {
            return ExecuteAndParseAsync(executor => executor.GlobalToolsPath(directory));
        }
    }
}
