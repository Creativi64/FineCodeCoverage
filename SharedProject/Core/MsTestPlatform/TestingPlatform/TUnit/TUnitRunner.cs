using FineCodeCoverage.Output;
using Microsoft.VisualStudio.Threading;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    [Export(typeof(ITUnitRunner))]
    internal class TUnitRunner : ITUnitRunner
    {
        private readonly ILogger logger;

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
                await logger.LogAsync("-----------");
                return process.ExitCode == 0;
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
