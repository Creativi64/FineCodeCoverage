using System.Threading.Tasks;

namespace FineCodeCoverage.Core.Initialization
{
    internal interface IShellPackageLoader
    {
        Task LoadPackageAsync();
    }
}