using System.Threading;
using System.Threading.Tasks;

namespace FineCodeCoverage.Initialization
{
    internal interface IFirstTimeToolWindowOpener
    {
        Task OpenIfFirstTimeAsync(CancellationToken cancellationToken);
    }
}
