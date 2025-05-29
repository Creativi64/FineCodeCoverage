using System.ComponentModel.Composition;
using System.Threading;

namespace FineCodeCoverage.Core.Utilities
{
    [Export(typeof(IToolUnzipper))]
    internal class ToolUnzipper : IToolUnzipper
    {
        private readonly IToolZipProvider toolZipProvider;
        private readonly IToolFolder toolFolder;

        [ImportingConstructor]
        public ToolUnzipper(
            IToolZipProvider toolZipProvider,
            IToolFolder toolFolder
            )
        {
            this.toolZipProvider = toolZipProvider;
            this.toolFolder = toolFolder;
        }
        public string EnsureUnzipped(string appDataFolder, string ownFolderName, string zipPrefix, CancellationToken cancellationToken)
        {
            return this.toolFolder.EnsureUnzipped(appDataFolder, ownFolderName, this.toolZipProvider.ProvideZip(zipPrefix), cancellationToken);
        }
    }
}
