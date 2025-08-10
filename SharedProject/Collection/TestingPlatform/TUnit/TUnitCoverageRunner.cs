using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Initialization;
using FineCodeCoverage.Initialization.ToolZip;
using FineCodeCoverage.VSAbstractions.OutputWindow;
using FineCodeCoverage.VSAbstractions.Threading;

namespace FineCodeCoverage.Collection.TestingPlatform.TUnit
{
    [Export(typeof(ITUnitCoverageRunner))]
    [Export(typeof(IAppDataFolderPathDependent))]
    internal sealed class TUnitCoverageRunner : ITUnitCoverageRunner, IAppDataFolderPathDependent
    {
        private const string ZipDirectoryName = "dotnet-coverage";
        private const string ZipPrefix = "dotnet-coverage";
        private const int SuccessExitCode = 0;
        private readonly ILogger _logger;
        private readonly IToolUnzipper _toolUnzipper;
        private readonly IThreadHelper _threadHelper;
        private readonly Dictionary<int, string> _nonSuccessExitCodeMessages = new Dictionary<int, string>
        {
            { 2, "At least one test failure." },
            { 3, "Test session was aborted." },
            { 4, "Setup of used extension is invalid." },
            { 5, "Command line arguments are invalid." },
            { 6, "Test session is using a non-implemented feature." },
            { 7, "Test session was unable to complete successfully, and likely crashed. It's possible that this was caused by a test session that was run via a test controller's extension point." },

            // todo check the source for this one as may be the minimum expected tests setting
            { 8, "Test session ran 0 tests." },
            { 9, "Minimum execution policy for the executed tests was violated." },
            { 10, "The test adapter failed to run tests for an infrastructure reason unrelated to the test's self.  An example is failing to create a fixture needed by tests." },
            { 11, "The test process will exit if dependent process exits" },
            { 12, "Test session was unable to run because the client does not support any of the supported protocol versions." },
            { 13, "Test session was stopped due to reaching the specified number of maximum failed tests using --maximum-failed-tests command-line option." },
        };

        private CancellationToken _cancellationToken;
        private string _dotnetCoverageExePath;

        public event EventHandler ReadyEvent;

        [ImportingConstructor]
        public TUnitCoverageRunner(
            ILogger logger,
            IToolUnzipper toolUnzipper,
            IThreadHelper threadHelper)
        {
            _logger = logger;
            _toolUnzipper = toolUnzipper;
            _threadHelper = threadHelper;
        }

        private (string, string) GetExeAndArgs(
            TUnitSettings tUnitSettings,
            bool hasCoverageExtension)
        {
            string path = hasCoverageExtension ? tUnitSettings.ExePath : _dotnetCoverageExePath;
            string args = hasCoverageExtension ? $"--disable-logo --coverage --coverage-output-format cobertura --coverage-settings \"{tUnitSettings.SettingsPath}\" --coverage-output  \"{tUnitSettings.OutputPath}\"" :
                    $"collect \"{tUnitSettings.ExePath}\" --disable-logo -f cobertura -o \"{tUnitSettings.OutputPath}\" -s \"{tUnitSettings.SettingsPath}\" --nologo";
            args = $"{args} {tUnitSettings.AdditionalArgs}";
            return (path, args);
        }

        public async Task<bool> RunAsync(
            TUnitSettings tUnitSettings,
            bool hasCoverageExtension,
            bool showWindow = false,
            CancellationToken cancellationToken = default)
        {
            _cancellationToken = cancellationToken;
            (string path, string args) = GetExeAndArgs(tUnitSettings, hasCoverageExtension);

            // could have FCC option - hide-test-output or just allow them to supply their own
            await _logger.LogAsync("Executing TUnit", path, "Arguments", args);
            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = path,
                    Arguments = args,
                    UseShellExecute = false,
                    CreateNoWindow = !showWindow,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                };
                process.OutputDataReceived += Process_OutputDataReceived;
                process.ErrorDataReceived += Process_ErrorDataReceived;
                cancellationToken.ThrowIfCancellationRequested();
                _ = process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                _ = await _threadHelper.WaitForForProcessExitAsync(process, cancellationToken);
                _ = process.WaitForExit(1000); // Ensures all output is handled

                /*
                    from https://learn.microsoft.com/en-us/dotnet/core/testing/microsoft-testing-platform-intro?tabs=dotnetcli#run-and-debug-tests
                    The app exits with a nonzero exit code if there's an error, which is typical for most executables. For more information on the known exit codes, see Microsoft.Testing.Platform exit codes.
                    Tip
                    You can ignore a specific exit code using the --ignore-exit-code command line option.

                */
                await LogNonSuccessExitCodeAsync(process.ExitCode);
                await _logger.LogAsync("-----------");
                return process.ExitCode == SuccessExitCode;
            }
        }

        private async Task LogNonSuccessExitCodeAsync(int exitCode)
        {
            if (exitCode == SuccessExitCode)
            {
                return;
            }

            string message = $"Non success exit code : {exitCode}.";
            if (_nonSuccessExitCodeMessages.TryGetValue(exitCode, out string msg))
            {
                message = $"{message}  {msg}";
            }

            await _logger.LogAsync(message);
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
            {
                return;
            }

            _logger.LogFileAndForget($"Error: {e.Data}");
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (_cancellationToken.IsCancellationRequested)
            {
                return;
            }

            _logger.LogFileAndForget(e.Data);
        }

        public Task InitializeAsync(string appDataFolderPath, CancellationToken cancellationToken)
        {
            string zipDestination = _toolUnzipper.EnsureUnzipped(appDataFolderPath, ZipDirectoryName, ZipPrefix, cancellationToken);
            _dotnetCoverageExePath = Directory.GetFiles(zipDestination, "dotnet-coverage.exe", SearchOption.AllDirectories).First();
            ReadyEvent?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }
    }
}
