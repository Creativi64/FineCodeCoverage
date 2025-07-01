using System.Collections.Generic;

namespace FineCodeCoverage.Utilities.DotNetToolList
{
    public interface IDotNetConfigFinder
    {
        IEnumerable<string> GetConfigDirectories(string upFromDirectory);
    }
}
