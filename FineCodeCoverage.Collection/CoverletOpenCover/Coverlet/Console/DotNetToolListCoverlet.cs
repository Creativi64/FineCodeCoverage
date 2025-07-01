using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using FineCodeCoverage.Output;
using FineCodeCoverage.Utilities.DotNetToolList;

namespace FineCodeCoverage.Collection.CoverletOpenCover.Coverlet.Console
{
    [Export(typeof(IDotNetToolListCoverlet))]
    internal sealed class DotNetToolListCoverlet : IDotNetToolListCoverlet
    {
        private const string CoverletPackageId = "coverlet.console";
        private readonly ILogger _logger;
        private readonly IDotNetToolListExecutor _executor;
        private readonly IDotNetToolListParser _parser;

        [ImportingConstructor]
        public DotNetToolListCoverlet(
            ILogger logger,
            IDotNetToolListExecutor executor,
            IDotNetToolListParser parser)
        {
            _logger = logger;
            _executor = executor;
            _parser = parser;
        }

        private async Task<CoverletDotNetToolDetails> ExecuteAndParseAsync(Func<IDotNetToolListExecutor, DotNetToolListExecutionResult> execution)
        {
            const string title = "Dotnet tool list Coverlet";
            DotNetToolListExecutionResult result = execution(_executor);
            if (result.ExitCode != 0)
            {
                await _logger.LogAsync($"{title} Error", result.Output);
                return null;
            }

            List<DotNetToolInfo> tools = null;
            try
            {
                tools = _parser.Parse(result.Output);
            }
            catch (Exception)
            {
                await _logger.LogAsync($"{title} Error parsing", result.Output);
                return null;
            }

            DotNetToolInfo coverletConsoleTool = tools.FirstOrDefault(tool => tool.PackageId == CoverletPackageId);
            return coverletConsoleTool == null
                ? null
                : new CoverletDotNetToolDetails
                {
                    Version = coverletConsoleTool.Version,
                    Command = coverletConsoleTool.Commands,
                };
        }

        public Task<CoverletDotNetToolDetails> GlobalAsync() => ExecuteAndParseAsync(executor => executor.Global());

        public Task<CoverletDotNetToolDetails> LocalAsync(string directory)
            => ExecuteAndParseAsync(executor => executor.Local(directory));

        public Task<CoverletDotNetToolDetails> GlobalToolsPathAsync(string directory)
            => ExecuteAndParseAsync(executor => executor.GlobalToolsPath(directory));
    }
}
