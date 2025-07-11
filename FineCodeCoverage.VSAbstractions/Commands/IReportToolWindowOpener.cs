using System.Threading.Tasks;

namespace FineCodeCoverage.VSAbstractions.Commands
{
    public interface IReportToolWindowOpener
    {
        Task TryOpenAsync();
    }
}
