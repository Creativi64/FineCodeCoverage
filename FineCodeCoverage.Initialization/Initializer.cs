using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Output;

namespace FineCodeCoverage.Initialization
{
    [Export(typeof(IInitializer))]
    [Export(typeof(IInitializeStatusProvider))]
    internal sealed class Initializer : IInitializer
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
            IInitializable[] initializables)
#pragma warning restore RCS1163 // Unused parameter
        {
            _appDataFolder = appDataFolder;
            _appDataFolderPathDependents = appDataFolderPathDependents;
            _logger = logger;
            _firstTimeToolWindowOpener = firstTimeToolWindowOpener;
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                await _logger.LogAsync("Initializing");

                cancellationToken.ThrowIfCancellationRequested();
                await _appDataFolder.InitializeAsync(cancellationToken);
                foreach (IAppDataFolderPathDependent appDataPathDependent in _appDataFolderPathDependents)
                {
                    await appDataPathDependent.InitializeAsync(_appDataFolder.DirectoryPath, cancellationToken);
                }

                cancellationToken.ThrowIfCancellationRequested();
                await _logger.LogAsync("Initialized");

                await _firstTimeToolWindowOpener.OpenIfFirstTimeAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                InitializeStatus = InitializeStatus.Error;
                InitializeExceptionMessage = exception.Message;
                if (!cancellationToken.IsCancellationRequested)
                {
                    await _logger.LogAsync("Failed Initialization", exception.ToString());
                }
            }

            if (InitializeStatus == InitializeStatus.Error)
            {
                return;
            }

            InitializeStatus = InitializeStatus.Initialized;
        }
    }
}
