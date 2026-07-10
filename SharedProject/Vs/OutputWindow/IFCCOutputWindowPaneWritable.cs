using System.Threading.Tasks;

namespace FineCodeCoverage.Vs.OutputWindow
{
    internal interface IFCCOutputWindowPaneWritable
    {
        Task OutputStringThreadSafeAsync(string text);
    }
}
