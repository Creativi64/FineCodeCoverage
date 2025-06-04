using System.IO;
using System.Linq;

namespace FineCodeCoverage.Engine.Coverlet
{
    public class CoverletConsoleExeFinder
    {
        public string FindInFolder(string folder, SearchOption searchOption)
            => Directory.GetFiles(folder, "coverlet.exe", searchOption).FirstOrDefault()
                ?? Directory.GetFiles(folder, "*coverlet*.exe", searchOption).FirstOrDefault();
    }
}
