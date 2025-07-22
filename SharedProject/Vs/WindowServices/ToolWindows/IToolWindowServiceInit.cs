using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;

namespace FineCodeCoverage.Output
{
    internal interface IToolWindowServiceInit
    {
        Task<IToolWindowService> InitializeAsync(AsyncPackage package);
    }
}
