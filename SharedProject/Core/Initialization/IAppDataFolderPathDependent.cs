using System.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverage.Core.Initialization
{
    internal interface IAppDataFolderPathDependent
    {
        Task InitializeAsync(string appDataFolderPath, CancellationToken cancellationToken);
    }
}