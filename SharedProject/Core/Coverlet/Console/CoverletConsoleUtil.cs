using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Output;

namespace FineCodeCoverage.Engine.Coverlet
{
    [Export(typeof(ICoverletConsoleUtil))]
    internal class CoverletConsoleUtil : ICoverletConsoleUtil
    {
        private readonly IProcessUtil _processUtil;
        private readonly ILogger _logger;
        private readonly ICoverletConsoleExecuteRequestProvider _coverletConsoleExecuteRequestProvider;
        private readonly IFCCCoverletConsoleExecutor _fccExecutor;
        private readonly ICoverletExeArgumentsProvider _coverletExeArgumentsProvider;

        [ImportingConstructor]
        public CoverletConsoleUtil(
            IProcessUtil processUtil,
            ILogger logger,
            ICoverletConsoleExecuteRequestProvider coverletConsoleExecuteRequestProvider,
            IFCCCoverletConsoleExecutor fccExecutor,
            ICoverletExeArgumentsProvider coverletExeArgumentsProvider
            )
        {
            this._processUtil = processUtil;
            this._logger = logger;
            this._coverletConsoleExecuteRequestProvider = coverletConsoleExecuteRequestProvider;
            this._fccExecutor = fccExecutor;
            this._coverletExeArgumentsProvider = coverletExeArgumentsProvider;
        }
        public void Initialize(string appDataFolder, CancellationToken cancellationToken)
            => this._fccExecutor.Initialize(appDataFolder, cancellationToken);

        public async Task RunAsync(ICoverageProject project, CancellationToken cancellationToken)
        {
            string title = $"Coverlet Run ({project.ProjectName})";

            List<string> coverletSettings = this._coverletExeArgumentsProvider.GetArguments(project);

            var executingLogLines = new List<string> { $"{title} - Arguments" };
            executingLogLines.AddRange(coverletSettings);
            await this._logger.LogAsync(executingLogLines);

            ExecuteResponse result = await this._processUtil.ExecuteAsync(
                await this._coverletConsoleExecuteRequestProvider.GetExecuteRequestAsync(project, string.Join(" ", coverletSettings)),
                cancellationToken
            );

            /*
			0 - Success.
			1 - If any test fails.
			2 - Coverage percentage is below threshold.
			3 - Test fails and also coverage percentage is below threshold.
			*/
            if (result.ExitCode > 3)
            {
                string errorExitCodeMessage = $"Error. Exit code: {result.ExitCode}";
                await this._logger.LogAsync($"{title} {errorExitCodeMessage}", result.Output);

                throw new CoverletExitCodeFailureException(errorExitCodeMessage);
            }

            await this._logger.LogAsync($"{title} - Output", result.Output);
        }
    }
}