using System.Collections.Generic;

namespace FineCodeCoverage.Core.Utilities
{
    public interface IDotNetConfigFinder
    {
        IEnumerable<string> GetConfigDirectories(string upFromDirectory);
    }
}
