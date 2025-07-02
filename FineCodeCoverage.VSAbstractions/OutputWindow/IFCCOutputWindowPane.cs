using System.Threading.Tasks;

namespace FineCodeCoverage.Output.Pane
{
    public interface IFCCOutputWindowPane
    {
        Task ShowAsync();

        Task<string> GetTextAsync();
    }
}
