using System.Threading.Tasks;

namespace FineCodeCoverage.Initialization
{
    public interface IPackageLoader
    {
        Task LoadPackageAsync(System.Threading.CancellationToken cancellationToken);
    }
}
