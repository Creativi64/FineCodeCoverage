using System.IO;

namespace FineCodeCoverage.Engine.Coverlet
{
    public interface ICoverletConsoleExeFinder
    {
        string FindInFolder(string folder, SearchOption searchOption);
    }
}