using System.IO;

namespace FineCodeCoverage.Collection.CoverletOpenCover.Coverlet.Console
{
    public interface ICoverletConsoleExeFinder
    {
        string FindInFolder(string folder, SearchOption searchOption);
    }
}
