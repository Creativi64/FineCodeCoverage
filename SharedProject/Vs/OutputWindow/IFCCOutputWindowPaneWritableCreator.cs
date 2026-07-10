using System.Threading.Tasks;

namespace FineCodeCoverage.Vs.OutputWindow
{
    internal interface IFCCOutputWindowPaneWritableCreator
    {
        Task<IFCCOutputWindowPaneWritable> GetOrCreateWritableAsync();
    }
}
