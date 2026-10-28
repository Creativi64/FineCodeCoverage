using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Initialization;
using FineCodeCoverage.Initialization.ToolZip;
using FineCodeCoverage.VSAbstractions.OutputWindow;
using FineCodeCoverage.VSAbstractions.Threading;
using Microsoft.Win32.SafeHandles;

namespace FineCodeCoverage.Collection.TestingPlatform.TUnit
{
    [Export(typeof(ITUnitCoverageRunner))]
    [Export(typeof(IAppDataFolderPathDependent))]
    internal sealed class TUnitCoverageRunner : ITUnitCoverageRunner, IAppDataFolderPathDependent
    {
        private const string ZipDirectoryName = "dotnet-coverage";
        private const string ZipPrefix = "dotnet-coverage";
        private const int SuccessExitCode = 0;

        // The Microsoft.Testing.Platform test host writes its console output (banner, localized summary text)
        // using the OEM code page (e.g. CP850 on German Windows, CP437 on US).  When the streams are redirected
        // we must decode them with that same code page, otherwise box-drawing characters and accented letters
        // turn into mojibake (e.g. the banner showing as "ÛÛÛÛ»" and "übersprungen" losing its "ü").
        private static readonly System.Text.Encoding OemOutputEncoding =
            System.Text.Encoding.GetEncoding(System.Globalization.CultureInfo.CurrentCulture.TextInfo.OEMCodePage);
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

            // Note: do not pass --disable-logo - it is a TUnit-specific option that other Microsoft.Testing.Platform
            // hosts (e.g. MSTest) reject with "Unknown option" (exit code 5).  The banner it suppressed is harmless.
            string args = hasCoverageExtension ? $"--coverage --coverage-output-format {tUnitSettings.OutputFormat} --coverage-settings \"{tUnitSettings.SettingsPath}\" --coverage-output  \"{tUnitSettings.OutputPath}\"" :
                    $"collect \"{tUnitSettings.ExePath}\" -f {tUnitSettings.OutputFormat} -o \"{tUnitSettings.OutputPath}\" -s \"{tUnitSettings.SettingsPath}\" --nologo";
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
            await _logger.LogAsync("Executing Microsoft.Testing.Platform test host", path, "Arguments", args);
            using (var processJob = new ProcessJob())
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
                    StandardOutputEncoding = OemOutputEncoding,
                    StandardErrorEncoding = OemOutputEncoding,
                };
                process.OutputDataReceived += Process_OutputDataReceived;
                process.ErrorDataReceived += Process_ErrorDataReceived;
                cancellationToken.ThrowIfCancellationRequested();
                _ = process.Start();
                try
                {
                    processJob.Attach(process);
                }
                catch
                {
                    if (!process.HasExited)
                    {
                        process.Kill();
                    }

                    throw;
                }
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await WaitForExitAsync(process, cancellationToken);
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

        internal async Task WaitForExitAsync(Process process, CancellationToken cancellationToken)
        {
            try
            {
                _ = await _threadHelper.WaitForForProcessExitAsync(process, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // The VS threading helper cancels only its wait, leaving the test host running.
                // This process belongs to this collection run, so terminate it before disposal.
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill();
                    }
                }
                catch (InvalidOperationException)
                {
                    // The host may exit between HasExited and Kill.
                }

                _ = process.WaitForExit(5000);
                throw;
            }
        }

        // MTP coverage can spawn an instrumented child host. Killing only the controller
        // leaves that child alive. A private Windows job owns this run's process tree.
        internal sealed class ProcessJob : IDisposable
        {
            private readonly SafeFileHandle _handle;

            public ProcessJob()
            {
                _handle = CreateJobObject(IntPtr.Zero, null);
                if (_handle.IsInvalid)
                {
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                }

                var limits = new ExtendedLimitInformation();
                limits.BasicLimitInformation.LimitFlags = 0x2000; // JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE
                if (!SetInformationJobObject(_handle, 9, ref limits, (uint)Marshal.SizeOf(limits)))
                {
                    int error = Marshal.GetLastWin32Error();
                    _handle.Dispose();
                    throw new System.ComponentModel.Win32Exception(error);
                }
            }

            public void Attach(Process process)
            {
                if (!AssignProcessToJobObject(_handle, process.Handle))
                {
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                }
            }

            public void Dispose() => _handle.Dispose();

            [StructLayout(LayoutKind.Sequential)]
            private struct BasicLimitInformation
            {
                public long PerProcessUserTimeLimit;
                public long PerJobUserTimeLimit;
                public uint LimitFlags;
                public UIntPtr MinimumWorkingSetSize;
                public UIntPtr MaximumWorkingSetSize;
                public uint ActiveProcessLimit;
                public UIntPtr Affinity;
                public uint PriorityClass;
                public uint SchedulingClass;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct IoCounters
            {
                public ulong ReadOperationCount;
                public ulong WriteOperationCount;
                public ulong OtherOperationCount;
                public ulong ReadTransferCount;
                public ulong WriteTransferCount;
                public ulong OtherTransferCount;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct ExtendedLimitInformation
            {
                public BasicLimitInformation BasicLimitInformation;
                public IoCounters IoInfo;
                public UIntPtr ProcessMemoryLimit;
                public UIntPtr JobMemoryLimit;
                public UIntPtr PeakProcessMemoryUsed;
                public UIntPtr PeakJobMemoryUsed;
            }

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            private static extern SafeFileHandle CreateJobObject(IntPtr attributes, string name);

            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool SetInformationJobObject(SafeFileHandle job, int informationClass, ref ExtendedLimitInformation information, uint length);

            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool AssignProcessToJobObject(SafeFileHandle job, IntPtr process);
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
