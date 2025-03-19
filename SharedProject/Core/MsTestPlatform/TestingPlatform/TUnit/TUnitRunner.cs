using Microsoft.VisualStudio.Threading;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    [Export(typeof(ITUnitRunner))]
    internal class TUnitRunner : ITUnitRunner
    {
        public async Task<bool> RunAsync(string exePath, string settingsPath, string outputpath)
        {
            using (var process = new Process())
            {
                process.StartInfo.FileName = exePath;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.Arguments = $"--coverage --coverage-output-format cobertura --coverage-output \"{outputpath}\"";
                // Optionally set other start info properties (e.g. arguments, CreateNoWindow, etc.)
                process.Start();
                //todo cancellation token
                await process.WaitForExitAsync();

                /*
                    from https://learn.microsoft.com/en-us/dotnet/core/testing/microsoft-testing-platform-intro?tabs=dotnetcli#run-and-debug-tests
                	The app exits with a nonzero exit code if there's an error, which is typical for most executables. For more information on the known exit codes, see Microsoft.Testing.Platform exit codes.
					Tip
				    You can ignore a specific exit code using the --ignore-exit-code command line option.

                */
                return process.ExitCode == 0;
            }
        }
    }
}
