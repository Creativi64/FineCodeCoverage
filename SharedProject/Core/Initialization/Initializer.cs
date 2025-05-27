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
                await logger.LogAsync("Initializing");

                cancellationToken.ThrowIfCancellationRequested();
                await appDataFolder.InitializeAsync(cancellationToken);
                foreach (var appDataPathDependent in appDataFolderPathDependents)
                {
                    await appDataPathDependent.InitializeAsync(appDataFolder.DirectoryPath, cancellationToken);
                }

                cancellationToken.ThrowIfCancellationRequested();
                await logger.LogAsync("Initialized");

                await firstTimeToolWindowOpener.OpenIfFirstTimeAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                InitializeStatus = InitializeStatus.Error;
                InitializeExceptionMessage = exception.Message;
                if (!cancellationToken.IsCancellationRequested)
                {
                    await logger.LogAsync("Failed Initialization", exception.ToString());
                }
            }

            if (InitializeStatus != InitializeStatus.Error)
            {
                InitializeStatus = InitializeStatus.Initialized;
            }
        }
    }

}

