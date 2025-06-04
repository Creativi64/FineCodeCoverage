using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Engine;
using FineCodeCoverage.Output;

namespace FineCodeCoverage.Core.Initialization
{
    [Export(typeof(IInitializer))]
    [Export(typeof(IInitializeStatusProvider))]
    internal class Initializer : IInitializer
    {
        private readonly IAppDataFolder _appDataFolder;
        private readonly IAppDataFolderPathDependent[] _appDataFolderPathDependents;
        private readonly ILogger _logger;
        private readonly IFirstTimeToolWindowOpener _firstTimeToolWindowOpener;

        public InitializeStatus InitializeStatus { get; set; } = InitializeStatus.Initializing;
        public string InitializeExceptionMessage { get; set; }

        [ImportingConstructor]
        public Initializer(
            IAppDataFolder appDataFolder,
            [ImportMany]
            IAppDataFolderPathDependent[] appDataFolderPathDependents,
            ILogger logger,
            IFirstTimeToolWindowOpener firstTimeToolWindowOpener,
#pragma warning disable RCS1163 // Unused parameter
            [ImportMany]
            IInitializable[] initializables
#pragma warning restore RCS1163 // Unused parameter
        )
        {
            this._appDataFolder = appDataFolder;
            this._appDataFolderPathDependents = appDataFolderPathDependents;
            this._logger = logger;
            this._firstTimeToolWindowOpener = firstTimeToolWindowOpener;
        }
        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                await this._logger.LogAsync("Initializing");

                cancellationToken.ThrowIfCancellationRequested();
                await this._appDataFolder.InitializeAsync(cancellationToken);
                foreach (IAppDataFolderPathDependent appDataPathDependent in this._appDataFolderPathDependents)
                {
                    await appDataPathDependent.InitializeAsync(this._appDataFolder.DirectoryPath, cancellationToken);
                }

                cancellationToken.ThrowIfCancellationRequested();
                await this._logger.LogAsync("Initialized");

                await this._firstTimeToolWindowOpener.OpenIfFirstTimeAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                this.InitializeStatus = InitializeStatus.Error;
                this.InitializeExceptionMessage = exception.Message;
                if (!cancellationToken.IsCancellationRequested)
                {
                    await this._logger.LogAsync("Failed Initialization", exception.ToString());
                }
            }

            if (this.InitializeStatus == InitializeStatus.Error)
            {
                return;
            }

            this.InitializeStatus = InitializeStatus.Initialized;
        }
    }
}