using System.Threading.Tasks;

namespace FineCodeCoverage.Output
{
    internal interface ICommandInitializer
    {
        Task InitializeAsync(ICommandPackageServices commandPackageServices);
    }
}
