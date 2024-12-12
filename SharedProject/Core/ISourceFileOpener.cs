using System.Threading.Tasks;

namespace FineCodeCoverage.Engine
{
    internal interface ISourceFileOpener
    {
        Task OpenAsync(string filePath, int fileLine);
    }

}