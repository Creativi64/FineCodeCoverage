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
        private readonly ILogger _logger;
        private readonly IEnvironmentVariable _environmentVariable;
        private readonly IOptionsProvider<MiscOptions> _toolsOptionsProvider;
        internal const string fccDebugCleanInstallEnvironmentVariable = "FCCDebugCleanInstall";

        [ImportingConstructor]
        public AppDataFolder(
            ILogger logger,
            IEnvironmentVariable environmentVariable,
            IOptionsProvider<MiscOptions> toolsOptionsProvider
        )
        {
            this._logger = logger;
            this._environmentVariable = environmentVariable;
            this._toolsOptionsProvider = toolsOptionsProvider;
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
            if (this._environmentVariable.Get(fccDebugCleanInstallEnvironmentVariable) != null)
            {
                await this._logger.LogAsync("FCCDebugCleanInstall");
                if (Directory.Exists(this.DirectoryPath))
                {
                    try
                    {
                        Directory.Delete(this.DirectoryPath, true);
                        await this._logger.LogAsync("Deleted app data folder");
                    }
                    catch (Exception exc)
                    {
                        await this._logger.LogAsync("Error deleting app data folder", exc.ToString());
                    }
                }
                else
                {
                    await this._logger.LogAsync("App data folder does not exist");
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
            string dir = this._toolsOptionsProvider.Get().ToolsDirectory;

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