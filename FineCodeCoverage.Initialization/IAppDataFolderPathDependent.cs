using System.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverage.Initialization
{
    public interface IAppDataFolderPathDependent
    {
        Task InitializeAsync(string appDataFolderPath, CancellationToken cancellationToken);
    }
}
