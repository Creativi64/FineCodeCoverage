using System.Threading;

namespace FineCodeCoverage.Initialization.ToolZip
{
    public interface IToolUnzipper
    {
        string EnsureUnzipped(string appDataFolder, string toolFolderName, string zipPrefix, CancellationToken cancellationToken);
    }
}
