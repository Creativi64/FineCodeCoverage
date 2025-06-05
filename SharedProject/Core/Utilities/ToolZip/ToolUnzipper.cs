using System.ComponentModel.Composition;
using System.Threading;

namespace FineCodeCoverage.Core.Utilities
{
    [Export(typeof(IToolUnzipper))]
    internal class ToolUnzipper : IToolUnzipper
    {
        private readonly IToolZipProvider _toolZipProvider;
        private readonly IToolFolder _toolFolder;

        [ImportingConstructor]
        public ToolUnzipper(
            IToolZipProvider toolZipProvider,
            IToolFolder toolFolder
            )
        {
            _toolZipProvider = toolZipProvider;
            _toolFolder = toolFolder;
        }

        public string EnsureUnzipped(
            string appDataFolder,
            string ownFolderName,
            string zipPrefix,
            CancellationToken cancellationToken
        ) => _toolFolder.EnsureUnzipped(
            appDataFolder,
            ownFolderName,
            _toolZipProvider.ProvideZip(zipPrefix),
            cancellationToken);
    }
}
