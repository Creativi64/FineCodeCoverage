using System.Collections.Generic;

namespace FineCodeCoverage.Output
{
    internal interface IChangeset
    {
        List<int> GetLineNumbers(string filePath);
    }
}
