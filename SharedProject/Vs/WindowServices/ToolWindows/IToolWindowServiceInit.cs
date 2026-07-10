using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;

namespace FineCodeCoverage.Vs.WindowServices.ToolWindows
{
    internal interface IToolWindowServiceInit
    {
        Task<IToolWindowService> InitializeAsync(AsyncPackage package);
    }
}
