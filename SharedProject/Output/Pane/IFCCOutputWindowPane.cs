using System.Threading.Tasks;

namespace FineCodeCoverage.Output.Pane
{
    interface IFCCOutputWindowPane
    {
        Task ShowAsync();
        Task OutputStringThreadSafeAsync(string text);
        Task<string> GetTextAsync();
    }
}
