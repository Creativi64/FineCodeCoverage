using System.Threading.Tasks;

namespace FineCodeCoverage.Initialization
{
    public interface IShellPackageLoader
    {
        Task LoadPackageAsync();
    }
}
