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

        private async Task<CoverletDotNetToolDetails> ExecuteAndParseAsync(Func<IDotNetToolListExecutor, DotNetToolListExecutionResult> execution)
        {
            const string title = "Dotnet tool list Coverlet";
            DotNetToolListExecutionResult result = execution(this.executor);
            if (result.ExitCode != 0)
            {
                await this.logger.LogAsync($"{title} Error", result.Output);
                return null;
            }

            List<DotNetToolInfo> tools = null;
            try
            {
                tools = this.parser.Parse(result.Output);
            }
            catch (Exception)
            {
                await this.logger.LogAsync($"{title} Error parsing", result.Output);
                return null;
            }

            DotNetToolInfo coverletConsoleTool = tools.FirstOrDefault(tool => tool.PackageId == CoverletPackageId);
            return coverletConsoleTool == null
                ? null
                : new CoverletDotNetToolDetails
                {
                    Version = coverletConsoleTool.Version,
                    Command = coverletConsoleTool.Commands
                };
        }

        public Task<CoverletDotNetToolDetails> GlobalAsync() => this.ExecuteAndParseAsync(executor => executor.Global());

        public Task<CoverletDotNetToolDetails> LocalAsync(string directory)
            => this.ExecuteAndParseAsync(executor => executor.Local(directory));

        public Task<CoverletDotNetToolDetails> GlobalToolsPathAsync(string directory)
            => this.ExecuteAndParseAsync(executor => executor.GlobalToolsPath(directory));
    }
}
