using System.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverage.Initialization
{
    public interface IInitializer : IInitializeStatusProvider
    {
        Task InitializeAsync(CancellationToken cancellationToken);
    }
}
