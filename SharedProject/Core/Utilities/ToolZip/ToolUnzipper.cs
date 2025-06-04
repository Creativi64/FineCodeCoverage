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
            this._toolZipProvider = toolZipProvider;
            this._toolFolder = toolFolder;
        }
        public string EnsureUnzipped(
            string appDataFolder,
            string ownFolderName,
            string zipPrefix,
            CancellationToken cancellationToken
        ) => this._toolFolder.EnsureUnzipped(
            appDataFolder,
            ownFolderName,
            this._toolZipProvider.ProvideZip(zipPrefix),
            cancellationToken);
    }
}