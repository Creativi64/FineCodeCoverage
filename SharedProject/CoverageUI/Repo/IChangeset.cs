using System.Collections.Generic;

namespace FineCodeCoverage.Output
{
    public interface IChangeset
    {
        List<int> GetLineNumbers(string filePath);
    }
}
