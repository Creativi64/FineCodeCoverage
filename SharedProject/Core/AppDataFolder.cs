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
        private readonly IOptionsProvider<MiscOptions> toolsOptionsProvider;
        internal const string fccDebugCleanInstallEnvironmentVariable = "FCCDebugCleanInstall";

        [ImportingConstructor]
        public AppDataFolder(
            ILogger logger,
            IEnvironmentVariable environmentVariable,
            IOptionsProvider<OutputOptions> outputOptionsProvider,
            IOptionsProvider<MiscOptions> toolsOptionsProvider
        )
        {
            this.logger = logger;
            this.environmentVariable = environmentVariable;
            this.toolsOptionsProvider = toolsOptionsProvider;
        }
        public string DirectoryPath { get; private set; }

        public async Task InitializeAsync(CancellationToken camcellationToken)
        {
            camcellationToken.ThrowIfCancellationRequested();
            await this.CreateAppDataFolderAsync();

            camcellationToken.ThrowIfCancellationRequested();
            this.CleanupLegacyFolders();
        }

        private async Task CleanInstallDevAsync()
        {
            if (this.environmentVariable.Get(fccDebugCleanInstallEnvironmentVariable) != null)
            {
                await this.logger.LogAsync("FCCDebugCleanInstall");
                if (Directory.Exists(this.DirectoryPath))
                {
                    try
                    {
                        Directory.Delete(this.DirectoryPath, true);
                        await this.logger.LogAsync("Deleted app data folder");
                    }
                    catch (Exception exc)
                    {
                        await this.logger.LogAsync("Error deleting app data folder", exc.ToString());
                    }
                }
                else
                {
                    await this.logger.LogAsync("App data folder does not exist");
                }
            }
        }

        private async Task CreateAppDataFolderAsync()
        {
            this.DirectoryPath = Path.Combine(this.GetAppDataFolder(), Vsix.Code);
            await this.CleanInstallDevAsync();
            _ = Directory.CreateDirectory(this.DirectoryPath);
        }

        private string GetAppDataFolder()
        {
            string dir = this.toolsOptionsProvider.Get().ToolsDirectory;

            return Directory.Exists(dir) ? dir : Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }

        private void CleanupLegacyFolders() => Directory
            .GetDirectories(this.DirectoryPath, "*", SearchOption.TopDirectoryOnly)
            .Where(path =>
            {
                string name = Path.GetFileName(path);

                return name.Contains("__") || Guid.TryParse(name, out Guid _);
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