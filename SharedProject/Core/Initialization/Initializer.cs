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
        private readonly IAppDataFolder appDataFolder;
        private readonly IAppDataFolderPathDependent[] appDataFolderPathDependents;
        private readonly ILogger logger;
        private readonly IFirstTimeToolWindowOpener firstTimeToolWindowOpener;

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
            this.appDataFolder = appDataFolder;
            this.appDataFolderPathDependents = appDataFolderPathDependents;
            this.logger = logger;
            this.firstTimeToolWindowOpener = firstTimeToolWindowOpener;
        }
        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                await this.logger.LogAsync("Initializing");

                cancellationToken.ThrowIfCancellationRequested();
                await this.appDataFolder.InitializeAsync(cancellationToken);
                foreach (IAppDataFolderPathDependent appDataPathDependent in this.appDataFolderPathDependents)
                {
                    await appDataPathDependent.InitializeAsync(this.appDataFolder.DirectoryPath, cancellationToken);
                }

                cancellationToken.ThrowIfCancellationRequested();
                await this.logger.LogAsync("Initialized");

                await this.firstTimeToolWindowOpener.OpenIfFirstTimeAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                this.InitializeStatus = InitializeStatus.Error;
                this.InitializeExceptionMessage = exception.Message;
                if (!cancellationToken.IsCancellationRequested)
                {
                    await this.logger.LogAsync("Failed Initialization", exception.ToString());
                }
            }

            if (this.InitializeStatus != InitializeStatus.Error)
            {
                this.InitializeStatus = InitializeStatus.Initialized;
            }
        }
    }
}

