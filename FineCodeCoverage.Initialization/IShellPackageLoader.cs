using System.Threading.Tasks;

namespace FineCodeCoverage.Core.Initialization
{
    public interface IShellPackageLoader
    {
        Task LoadPackageAsync();
    }
}
