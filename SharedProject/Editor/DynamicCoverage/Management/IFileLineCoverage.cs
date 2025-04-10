using System.Collections.Generic;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal interface IFileLineCoverage
    {
        IEnumerable<ICoberturaLine> GetLines(string filePath);
    }
}
