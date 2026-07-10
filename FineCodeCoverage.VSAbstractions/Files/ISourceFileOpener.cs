using System.Threading.Tasks;

namespace FineCodeCoverage.VSAbstractions.Files
{
    public interface ISourceFileOpener
    {
        Task OpenAsync(string filePath, int fileLine);
    }
}
