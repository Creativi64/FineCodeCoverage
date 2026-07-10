using System.Threading.Tasks;

namespace FineCodeCoverage.Initialization
{
    internal interface IAppDataFolder
    {
        string DirectoryPath { get; }

        Task InitializeAsync(System.Threading.CancellationToken cancellationToken);
    }
}
