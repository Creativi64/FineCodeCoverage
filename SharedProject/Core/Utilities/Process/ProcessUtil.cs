using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;

namespace FineCodeCoverage.Core.Utilities
{
    [Export(typeof(IProcessUtil))]
    internal class ProcessUtil : IProcessUtil
    {
        public async Task<ExecuteResponse> ExecuteAsync(ExecuteRequest request, CancellationToken cancellationToken)
        {
            CommandTask<BufferedCommandResult> commandTask = Cli
            .Wrap(request.FilePath)
            .WithArguments(request.Arguments)
            .WithValidation(CommandResultValidation.None)
            .WithWorkingDirectory(request.WorkingDirectory)
            .ExecuteBufferedAsync(cancellationToken);

            BufferedCommandResult result = null;
            try
            {
                result = await commandTask;
            }
            catch (OperationCanceledException) // CliWrap does not use the same ct
            {
                cancellationToken.ThrowIfCancellationRequested();
            }

            int exitCode = result.ExitCode;

            List<string> outputList = new List<string>();

            string directOutput = string.Join(Environment.NewLine, new[]
            {
                result.StandardOutput,
                Environment.NewLine,
                result.StandardError
            }
            .Where(x => !string.IsNullOrWhiteSpace(x)))
            .Trim('\r', '\n')
            .Trim();

            if (!string.IsNullOrWhiteSpace(directOutput))
            {
                outputList.Add(directOutput);
            }

            string output = string.Join(Environment.NewLine, outputList)
            .Trim('\r', '\n')
            .Trim();

            return new ExecuteResponse
            {
                ExitCode = exitCode,
                ExitTime = result.ExitTime,
                RunTime = result.RunTime,
                StartTime = result.StartTime,
                Output = output
            };
        }
    }
}
