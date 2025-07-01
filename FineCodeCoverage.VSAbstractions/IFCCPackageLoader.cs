using System.Threading.Tasks;

namespace FineCodeCoverage.Initialization
{
    public interface IFCCPackageLoader
    {
        Task LoadPackageAsync();
    }
}
