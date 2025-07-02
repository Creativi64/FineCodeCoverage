using System.Threading.Tasks;

namespace FineCodeCoverage.Output.Pane
{
    public interface IFCCOutputWindowPane
    {
        Task ShowAsync();

        Task OutputStringThreadSafeAsync(string text);

        Task<string> GetTextAsync();
    }
}
