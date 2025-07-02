using System.Threading.Tasks;

namespace FineCodeCoverage.Output.Pane
{
    internal interface IFCCOutputWindowPaneWritable
    {
        Task OutputStringThreadSafeAsync(string text);
    }
}
