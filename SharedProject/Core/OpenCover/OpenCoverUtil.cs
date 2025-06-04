using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Engine.MsTestPlatform;
using FineCodeCoverage.Output;

namespace FineCodeCoverage.Engine.OpenCover
{
    [Export(typeof(IOpenCoverUtil))]
    internal class OpenCoverUtil : IOpenCoverUtil
    {
        private string _openCoverExePath;
        private readonly IMsTestPlatformUtil _msTestPlatformUtil;
        private readonly IProcessUtil _processUtil;
        private readonly ILogger _logger;
        private readonly IToolUnzipper _toolUnzipper;
        private readonly IFileUtil _fileUtil;
        private readonly IOpenCoverExeArgumentsProvider _openCoverExeArgumentsProvider;
        private const string zipPrefix = "openCover";
        private const string zipDirectoryName = "openCover";

        [ImportingConstructor]
        public OpenCoverUtil(
            IMsTestPlatformUtil msTestPlatformUtil,
            IProcessUtil processUtil,
            ILogger logger,
            IToolUnzipper toolUnzipper,
            IFileUtil fileUtil,
            IOpenCoverExeArgumentsProvider openCoverExeArgumentsProvider

        )
        {
            this._msTestPlatformUtil = msTestPlatformUtil;
            this._processUtil = processUtil;
            this._logger = logger;
            this._toolUnzipper = toolUnzipper;
            this._fileUtil = fileUtil;
            this._openCoverExeArgumentsProvider = openCoverExeArgumentsProvider;
        }

        public void Initialize(string appDataFolder, CancellationToken cancellationToken)
        {
            string zipDestination = this._toolUnzipper.EnsureUnzipped(appDataFolder, zipDirectoryName, zipPrefix, cancellationToken);
            this._openCoverExePath = this._fileUtil.GetFiles(zipDestination, "OpenCover.Console.exe", SearchOption.AllDirectories).First();
        }

        private string GetOpenCoverExePath(string customExePath)
            => !string.IsNullOrWhiteSpace(customExePath) ? customExePath : this._openCoverExePath;

        private void DeleteTestPdbIfDoNotIncludeTestAssembly(ICoverageProject project)
        {
            if (!project.Settings.IncludeTestAssembly)
            {
                // deleting the pdb of the test assembly seems to work; this is a VERY VERY shameful hack :(

                string testDllPdbFile = Path.Combine(project.ProjectOutputFolder, Path.GetFileNameWithoutExtension(project.TestDllFile)) + ".pdb";
                this._fileUtil.DeleteFile(testDllPdbFile);

                // filtering out the test-assembly blows up the entire process and nothing gets instrumented or analysed

                //var nameOnlyOfDll = Path.GetFileNameWithoutExtension(project.TestDllFileInWorkFolder);
                //filters.Add($@"-[{nameOnlyOfDll}]*");
            }
        }

        public async Task RunOpenCoverAsync(ICoverageProject project, CancellationToken cancellationToken)
        {
            this.DeleteTestPdbIfDoNotIncludeTestAssembly(project);

            System.Collections.Generic.List<string> openCoverSettings = this._openCoverExeArgumentsProvider.Provide(project, this._msTestPlatformUtil.MsTestPlatformExePath);

            string title = $"OpenCover Run ({project.ProjectName})";

            await this._logger.LogAsync($"{title} Arguments {Environment.NewLine}{string.Join($"{Environment.NewLine}", openCoverSettings)}");

            ExecuteResponse result = await this._processUtil
            .ExecuteAsync(new ExecuteRequest
            {
                FilePath = this.GetOpenCoverExePath(project.Settings.OpenCoverCustomPath),
                Arguments = string.Join(" ", openCoverSettings),
                WorkingDirectory = project.ProjectOutputFolder
            }, cancellationToken);

            if (result.ExitCode != 0)
            {
                throw new OpenCoverExitCodeException(result.Output);
            }

            await this._logger.LogAsync($"{title} - Output", result.Output);
        }
    }
}