using System.Threading.Tasks;

namespace FineCodeCoverage.Core.Initialization
{
    public interface IPackageLoader
    {
        Task LoadPackageAsync(System.Threading.CancellationToken cancellationToken);
    }
}
