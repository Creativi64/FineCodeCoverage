using System.Threading.Tasks;

namespace FineCodeCoverage.Output.Pane
{
    public interface IFCCOutputWindowPaneCreator
    {
        Task<IFCCOutputWindowPane> GetOrCreateAsync();
    }
}
