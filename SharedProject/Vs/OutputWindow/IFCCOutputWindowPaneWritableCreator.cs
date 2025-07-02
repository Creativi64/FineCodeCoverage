using System.Threading.Tasks;

namespace FineCodeCoverage.Output.Pane
{
    internal interface IFCCOutputWindowPaneWritableCreator
    {
        Task<IFCCOutputWindowPaneWritable> GetOrCreateWritableAsync();
    }
}
