using System.Threading.Tasks;

namespace FineCodeCoverage.VSAbstractions.OutputWindow
{
    public interface IFCCOutputWindowPaneCreator
    {
        Task<IFCCOutputWindowPane> GetOrCreateAsync();
    }
}
