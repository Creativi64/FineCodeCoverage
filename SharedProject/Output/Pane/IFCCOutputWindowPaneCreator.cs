using System.Threading.Tasks;

namespace FineCodeCoverage.Output.Pane
{
    internal interface IFCCOutputWindowPaneCreator
    {
        Task<IFCCOutputWindowPane> GetOrCreateAsync();
    }
}
