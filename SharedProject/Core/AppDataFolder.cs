using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Options;
using FineCodeCoverage.Output;

namespace FineCodeCoverage.Engine
{
    [Export(typeof(IAppDataFolder))]
    internal class AppDataFolder : IAppDataFolder
    {
        private readonly ILogger logger;
        private readonly IEnvironmentVariable environmentVariable;
        private readonly IAppOptionsProvider appOptionsProvider;
        internal const string fccDebugCleanInstallEnvironmentVariable = "FCCDebugCleanInstall";

        [ImportingConstructor]
        public AppDataFolder(ILogger logger,IEnvironmentVariable environmentVariable, IAppOptionsProvider appOptionsProvider)
        {
            this.logger = logger;
            this.environmentVariable = environmentVariable;
            this.appOptionsProvider = appOptionsProvider;
        }
        public string DirectoryPath { get; private set; }

        public async Task InitializeAsync(CancellationToken camcellationToken)
        {
            camcellationToken.ThrowIfCancellationRequested();
            await CreateAppDataFolderAsync();

            camcellationToken.ThrowIfCancellationRequested();
            CleanupLegacyFolders();
        }

        private async Task CleanInstallDevAsync()
        {
            if (environmentVariable.Get(fccDebugCleanInstallEnvironmentVariable) != null)
            {
                await logger.LogAsync("FCCDebugCleanInstall");
                if (Directory.Exists(DirectoryPath))
                {
                    try
                    {
                        Directory.Delete(DirectoryPath, true);
                        await logger.LogAsync("Deleted app data folder");
                    }
                    catch (Exception exc)
                    {
                        await logger.LogAsync("Error deleting app data folder", exc.ToString());
                    }
                }
                else
                {
                    await logger.LogAsync("App data folder does not exist");
                }
            }
        }

        private async Task CreateAppDataFolderAsync()
        {
            DirectoryPath = Path.Combine(GetAppDataFolder(), Vsix.Code);
            await CleanInstallDevAsync();
            Directory.CreateDirectory(DirectoryPath);
        }

        private string GetAppDataFolder()
        {
            var dir = appOptionsProvider.Get().ToolsDirectory;

            return Directory.Exists(dir) ? dir : Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }

        private void CleanupLegacyFolders()
        {
            Directory
            .GetDirectories(DirectoryPath, "*", SearchOption.TopDirectoryOnly)
            .Where(path =>
            {
                var name = Path.GetFileName(path);

                if (name.Contains("__"))
                {
                    return true;
                }

                if (Guid.TryParse(name, out var _))
                {
                    return true;
                }

                return false;
            })
            .ToList()
            .ForEach(path =>
            {
                try
                {
                    Directory.Delete(path, true);
                }
                catch
                {
                    // ignore
                }
            });
        }
    }
}