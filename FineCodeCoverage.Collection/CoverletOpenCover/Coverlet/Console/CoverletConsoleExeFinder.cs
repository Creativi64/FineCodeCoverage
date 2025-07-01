using System.IO;
using System.Linq;

namespace FineCodeCoverage.Collection.CoverletOpenCover.Coverlet.Console
{
    public class CoverletConsoleExeFinder
    {
        public string FindInFolder(string folder, SearchOption searchOption)
            => Directory.GetFiles(folder, "coverlet.exe", searchOption).FirstOrDefault()
                ?? Directory.GetFiles(folder, "*coverlet*.exe", searchOption).FirstOrDefault();
    }
}
