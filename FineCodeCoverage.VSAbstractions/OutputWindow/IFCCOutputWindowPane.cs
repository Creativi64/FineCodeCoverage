using System.Threading.Tasks;

namespace FineCodeCoverage.VSAbstractions.OutputWindow
{
    public interface IFCCOutputWindowPane
    {
        Task ShowAsync();

        Task<string> GetTextAsync();
    }
}
