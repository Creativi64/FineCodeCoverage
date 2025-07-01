using System.Threading.Tasks;

namespace FineCodeCoverage.Engine
{
    public interface ISourceFileOpener
    {
        Task OpenAsync(string filePath, int fileLine);
    }
}
