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
        private const string ZipPrefix = "openCover";
        private const string ZipDirectoryName = "openCover";
        private readonly IMsTestPlatformUtil _msTestPlatformUtil;
        private readonly IProcessUtil _processUtil;
        private readonly ILogger _logger;
        private readonly IToolUnzipper _toolUnzipper;
        private readonly IFileUtil _fileUtil;
        private readonly IOpenCoverExeArgumentsProvider _openCoverExeArgumentsProvider;
        private string _openCoverExePath;

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
            _msTestPlatformUtil = msTestPlatformUtil;
            _processUtil = processUtil;
            _logger = logger;
            _toolUnzipper = toolUnzipper;
            _fileUtil = fileUtil;
            _openCoverExeArgumentsProvider = openCoverExeArgumentsProvider;
        }

        public void Initialize(string appDataFolder, CancellationToken cancellationToken)
        {
            string zipDestination = _toolUnzipper.EnsureUnzipped(appDataFolder, ZipDirectoryName, ZipPrefix, cancellationToken);
            _openCoverExePath = _fileUtil.GetFiles(zipDestination, "OpenCover.Console.exe", SearchOption.AllDirectories).First();
        }

        private string GetOpenCoverExePath(string customExePath)
            => !string.IsNullOrWhiteSpace(customExePath) ? customExePath : _openCoverExePath;

        private void DeleteTestPdbIfDoNotIncludeTestAssembly(ICoverageProject project)
        {
            if (project.Settings.IncludeTestAssembly)
            {
                return;
            }

            string testDllPdbFile = Path.Combine(project.ProjectOutputFolder, Path.GetFileNameWithoutExtension(project.TestDllFile)) + ".pdb";
            _fileUtil.DeleteFile(testDllPdbFile);

            // deleting the pdb of the test assembly seems to work; this is a VERY VERY shameful hack :(
            // filtering out the test-assembly blows up the entire process and nothing gets instrumented or analysed

            // var nameOnlyOfDll = Path.GetFileNameWithoutExtension(project.TestDllFileInWorkFolder);
            // filters.Add($@"-[{nameOnlyOfDll}]*");
        }

        public async Task RunOpenCoverAsync(ICoverageProject project, CancellationToken cancellationToken)
        {
            DeleteTestPdbIfDoNotIncludeTestAssembly(project);

            System.Collections.Generic.List<string> openCoverSettings = _openCoverExeArgumentsProvider.Provide(project, _msTestPlatformUtil.MsTestPlatformExePath);

            string title = $"OpenCover Run ({project.ProjectName})";

            await _logger.LogAsync($"{title} Arguments {Environment.NewLine}{string.Join($"{Environment.NewLine}", openCoverSettings)}");

            ExecuteResponse result = await _processUtil
            .ExecuteAsync(
                new ExecuteRequest
                {
                    FilePath = GetOpenCoverExePath(project.Settings.OpenCoverCustomPath),
                    Arguments = string.Join(" ", openCoverSettings),
                    WorkingDirectory = project.ProjectOutputFolder,
                },
                cancellationToken
            );

            if (result.ExitCode != 0)
            {
                throw new OpenCoverExitCodeException(result.Output);
            }

            await _logger.LogAsync($"{title} - Output", result.Output);
        }
    }
}
