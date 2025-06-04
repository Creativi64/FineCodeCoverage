using System.Threading.Tasks;

namespace FineCodeCoverage.Output.Pane
{
    interface IFCCOutputWindowPaneCreator
    {
        Task<IFCCOutputWindowPane> GetOrCreateAsync();
    }
}
