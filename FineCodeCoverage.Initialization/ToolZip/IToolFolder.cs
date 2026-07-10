using System.Threading;

namespace FineCodeCoverage.Initialization.ToolZip
{
    internal interface IToolFolder
    {
        string EnsureUnzipped(string appDataFolder, string ownFolderName, ZipDetails zipDetails, CancellationToken cancellationToken);
    }
}
