using System.Threading.Tasks;

namespace FineCodeCoverage.Output.Pane
{
    internal interface IFCCOutputWindowPane
    {
        Task ShowAsync();
        Task OutputStringThreadSafeAsync(string text);
        Task<string> GetTextAsync();
    }
}
