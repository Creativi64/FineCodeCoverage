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
        private string openCoverExePath;
        private readonly IMsTestPlatformUtil msTestPlatformUtil;
        private readonly IProcessUtil processUtil;
        private readonly ILogger logger;
        private readonly IToolUnzipper toolUnzipper;
        private readonly IFileUtil fileUtil;
        private readonly IOpenCoverExeArgumentsProvider openCoverExeArgumentsProvider;
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
            this.msTestPlatformUtil = msTestPlatformUtil;
            this.processUtil = processUtil;
            this.logger = logger;
            this.toolUnzipper = toolUnzipper;
            this.fileUtil = fileUtil;
            this.openCoverExeArgumentsProvider = openCoverExeArgumentsProvider;
        }

        public void Initialize(string appDataFolder, CancellationToken cancellationToken)
        {
            string zipDestination = toolUnzipper.EnsureUnzipped(appDataFolder, zipDirectoryName, zipPrefix, cancellationToken);
            openCoverExePath = fileUtil.GetFiles(zipDestination, "OpenCover.Console.exe", SearchOption.AllDirectories).First();
        }

        private string GetOpenCoverExePath(string customExePath)
        {
            if (!String.IsNullOrWhiteSpace(customExePath))
            {
                return customExePath;
            }
            return openCoverExePath;
        }

        private void DeleteTestPdbIfDoNotIncludeTestAssembly(ICoverageProject project)
        {
            if (!project.Settings.IncludeTestAssembly)
            {
                // deleting the pdb of the test assembly seems to work; this is a VERY VERY shameful hack :(

                string testDllPdbFile = Path.Combine(project.ProjectOutputFolder, Path.GetFileNameWithoutExtension(project.TestDllFile)) + ".pdb";
                fileUtil.DeleteFile(testDllPdbFile);

                // filtering out the test-assembly blows up the entire process and nothing gets instrumented or analysed

                //var nameOnlyOfDll = Path.GetFileNameWithoutExtension(project.TestDllFileInWorkFolder);
                //filters.Add($@"-[{nameOnlyOfDll}]*");
            }
        }

        public async Task RunOpenCoverAsync(ICoverageProject project, CancellationToken cancellationToken)
        {
            DeleteTestPdbIfDoNotIncludeTestAssembly(project);

            System.Collections.Generic.List<string> openCoverSettings = openCoverExeArgumentsProvider.Provide(project, msTestPlatformUtil.MsTestPlatformExePath);

            string title = $"OpenCover Run ({project.ProjectName})";

            await logger.LogAsync($"{title} Arguments {Environment.NewLine}{string.Join($"{Environment.NewLine}", openCoverSettings)}");

            ExecuteResponse result = await processUtil
            .ExecuteAsync(new ExecuteRequest
            {
                FilePath = GetOpenCoverExePath(project.Settings.OpenCoverCustomPath),
                Arguments = string.Join(" ", openCoverSettings),
                WorkingDirectory = project.ProjectOutputFolder
            }, cancellationToken);

            if (result.ExitCode != 0)
            {
                throw new Exception(result.Output);
            }

            await logger.LogAsync($"{title} - Output", result.Output);
        }
    }
}
