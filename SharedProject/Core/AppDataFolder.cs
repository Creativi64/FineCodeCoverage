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
    internal sealed class AppDataFolder : IAppDataFolder
    {
        private readonly ILogger _logger;
        private readonly IEnvironmentVariable _environmentVariable;
        private readonly IOptionsProvider<MiscOptions> _toolsOptionsProvider;
        internal const string FCCDebugCleanInstallEnvironmentVariable = "FCCDebugCleanInstall";

        [ImportingConstructor]
        public AppDataFolder(
            ILogger logger,
            IEnvironmentVariable environmentVariable,
            IOptionsProvider<MiscOptions> toolsOptionsProvider)
        {
            _logger = logger;
            _environmentVariable = environmentVariable;
            _toolsOptionsProvider = toolsOptionsProvider;
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
            if (_environmentVariable.Get(FCCDebugCleanInstallEnvironmentVariable) == null)
            {
                return;
            }

            await _logger.LogAsync("FCCDebugCleanInstall");
            if (Directory.Exists(DirectoryPath))
            {
                try
                {
                    Directory.Delete(DirectoryPath, true);
                    await _logger.LogAsync("Deleted app data folder");
                }
                catch (Exception exc)
                {
                    await _logger.LogAsync("Error deleting app data folder", exc.ToString());
                }
            }
            else
            {
                await _logger.LogAsync("App data folder does not exist");
            }
        }

        private async Task CreateAppDataFolderAsync()
        {
            DirectoryPath = Path.Combine(GetAppDataFolder(), "FineCodeCoverage");
            await CleanInstallDevAsync();
            _ = Directory.CreateDirectory(DirectoryPath);
        }

        private string GetAppDataFolder()
        {
            string dir = _toolsOptionsProvider.Get().ToolsDirectory;

            return Directory.Exists(dir) ? dir : Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }

        private void CleanupLegacyFolders() => Directory
            .GetDirectories(DirectoryPath, "*", SearchOption.TopDirectoryOnly)
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
