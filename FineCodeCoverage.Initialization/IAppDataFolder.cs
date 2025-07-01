using System.Threading.Tasks;

namespace FineCodeCoverage.Engine
{
    internal interface IAppDataFolder
    {
        string DirectoryPath { get; }

        Task InitializeAsync(System.Threading.CancellationToken cancellationToken);
    }
}
