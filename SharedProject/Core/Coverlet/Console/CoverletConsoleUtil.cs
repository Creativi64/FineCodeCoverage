using System;
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
        private readonly IProcessUtil processUtil;
        private readonly ILogger logger;
        private readonly ICoverletConsoleExecuteRequestProvider coverletConsoleExecuteRequestProvider;
        private readonly IFCCCoverletConsoleExecutor fccExecutor;
        private readonly ICoverletExeArgumentsProvider coverletExeArgumentsProvider;

        [ImportingConstructor]
        public CoverletConsoleUtil(
            IProcessUtil processUtil,
            ILogger logger,
            ICoverletConsoleExecuteRequestProvider coverletConsoleExecuteRequestProvider,
            IFCCCoverletConsoleExecutor fccExecutor,
            ICoverletExeArgumentsProvider coverletExeArgumentsProvider
            )
        {
            this.processUtil = processUtil;
            this.logger = logger;
            this.coverletConsoleExecuteRequestProvider = coverletConsoleExecuteRequestProvider;
            this.fccExecutor = fccExecutor;
            this.coverletExeArgumentsProvider = coverletExeArgumentsProvider;
        }
        public void Initialize(string appDataFolder, CancellationToken cancellationToken)
        {
            fccExecutor.Initialize(appDataFolder, cancellationToken);
        }

        public async Task RunAsync(ICoverageProject project, CancellationToken cancellationToken)
        {
            var title = $"Coverlet Run ({project.ProjectName})";

            var coverletSettings = coverletExeArgumentsProvider.GetArguments(project);

            var executingLogLines = new List<string> { $"{title} - Arguments" };
            executingLogLines.AddRange(coverletSettings);
            await logger.LogAsync(executingLogLines);

            var result = await processUtil.ExecuteAsync(
                await coverletConsoleExecuteRequestProvider.GetExecuteRequestAsync(project, string.Join(" ", coverletSettings)),
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
                var errorExitCodeMessage = $"Error. Exit code: {result.ExitCode}";
                await logger.LogAsync($"{title} {errorExitCodeMessage}", result.Output);

                throw new Exception(errorExitCodeMessage);
            }

            await logger.LogAsync($"{title} - Output", result.Output);
        }
    }
}
