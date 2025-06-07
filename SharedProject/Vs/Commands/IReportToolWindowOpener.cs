using System.Threading.Tasks;

namespace FineCodeCoverage.Core.Utilities
{
    internal interface IReportToolWindowOpener
    {
        Task TryOpenAsync();
    }
}
