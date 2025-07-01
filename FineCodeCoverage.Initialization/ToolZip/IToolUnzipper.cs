using System.Threading;

namespace FineCodeCoverage.Core.Utilities
{
    public interface IToolUnzipper
    {
        string EnsureUnzipped(string appDataFolder, string toolFolderName, string zipPrefix, CancellationToken cancellationToken);
    }
}
