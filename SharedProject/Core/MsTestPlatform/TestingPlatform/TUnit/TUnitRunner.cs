using FineCodeCoverage.Output;
using Microsoft.VisualStudio.Threading;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    [Export(typeof(ITUnitRunner))]
    internal class TUnitRunner : ITUnitRunner
    {
        private readonly ILogger logger;
        private const int successExitCode = 0;
        private readonly Dictionary<int, string> nonSuccessExitCodeMessages = new Dictionary<int, string>
        {
            { 2, "At least one test failure." },
            { 3, "Test session was aborted." },
            { 4, "Setup of used extension is invalid."},
            { 5, "Command line arguments are invalid."},
            { 6, "Test session is using a non-implemented feature." },
            { 7, "Test session was unable to complete successfully, and likely crashed. It's possible that this was caused by a test session that was run via a test controller's extension point."},
            // todo check the source for this one as may be the minimum expected tests setting
            { 8, "Test session ran 0 tests." },
            { 9, "Minimum execution policy for the executed tests was violated." },
            { 10, "The test adapter failed to run tests for an infrastructure reason unrelated to the test's self.  An example is failing to create a fixture needed by tests." },
            { 11, "The test process will exit if dependent process exits" },
            { 12, "Test session was unable to run because the client does not support any of the supported protocol versions." },
            { 13, "Test session was stopped due to reaching the specified number of maximum failed tests using --maximum-failed-tests command-line option." }
        };

        [ImportingConstructor]
        public TUnitRunner(
            ILogger logger
        )
        {
            this.logger = logger;
        }
        public async Task<bool> RunAsync(string exePath, string settingsPath, string outputpath,bool showWindow = false)
        {
            // could have FCC option - hide-test-output or just allow them to supply their own
            var arguments = $"--disable-logo --coverage --coverage-output-format cobertura --coverage-output \"{outputpath}\"";
            await logger.LogAsync("Executing TUnit", exePath, "Arguments", arguments);
            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = !showWindow,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                };
                process.OutputDataReceived += Process_OutputDataReceived;
                process.ErrorDataReceived += Process_ErrorDataReceived;
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                //todo cancellation token
                await process.WaitForExitAsync();
                process.WaitForExit(); // Ensures all output is handled

                /*
                    from https://learn.microsoft.com/en-us/dotnet/core/testing/microsoft-testing-platform-intro?tabs=dotnetcli#run-and-debug-tests
                	The app exits with a nonzero exit code if there's an error, which is typical for most executables. For more information on the known exit codes, see Microsoft.Testing.Platform exit codes.
					Tip
				    You can ignore a specific exit code using the --ignore-exit-code command line option.

                */
                await LogNonSuccessExitCodeAsync(process.ExitCode);
                await logger.LogAsync("-----------");
                return process.ExitCode == successExitCode;
            }
        }

        private async Task LogNonSuccessExitCodeAsync(int exitCode)
        {
            if(exitCode != successExitCode)
            {
                string message = $"Non success exit code : {exitCode}.";
                if(nonSuccessExitCodeMessages.TryGetValue(exitCode, out var msg))
                {
                    message = $"{message}  {msg}";
                }
                await logger.LogAsync(message);
            }
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                logger.Log($"Error: {e.Data}");
            }
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            logger.Log(e.Data);
        }
    }
}
