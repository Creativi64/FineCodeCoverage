using System.Threading.Tasks;

namespace FineCodeCoverage.Vs.Commands.CommandInitializer
{
    internal interface ICommandInitializer
    {
        Task InitializeAsync(ICommandPackageServices commandPackageServices);
    }
}
