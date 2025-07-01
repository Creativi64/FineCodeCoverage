using System.Threading.Tasks;

namespace FineCodeCoverage.Core.Utilities
{
    public interface IReportToolWindowOpener
    {
        Task TryOpenAsync();
    }
}
