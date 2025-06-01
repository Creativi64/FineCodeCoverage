using System.ComponentModel.Composition;
using System.Diagnostics;

namespace FineCodeCoverage.Core.Utilities
{
    [Export(typeof(IDotNetToolListExecutor))]
    internal class DotNetToolListExecutor : IDotNetToolListExecutor
    {
        public DotNetToolListExecutionResult Global() => Execute("--global");

        public DotNetToolListExecutionResult Local(string directory) => Execute("--local", directory);

        public DotNetToolListExecutionResult GlobalToolsPath(string directory)
        {
            string safeDirectory = $@"""{directory}""";
            return Execute($"--tool-path {safeDirectory}");
        }

        private static DotNetToolListExecutionResult Execute(string additionalArguments, string workingDirectory = null)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = workingDirectory,
                Arguments = $"tool list {additionalArguments}",
            };

            if (workingDirectory != null)
            {
                processStartInfo.WorkingDirectory = workingDirectory;
            }

            var process = Process.Start(processStartInfo);

            process.WaitForExit();

            return new DotNetToolListExecutionResult
            {
                Output = process.GetOutput(),
                ExitCode = process.ExitCode
            };

        }
    }
}
