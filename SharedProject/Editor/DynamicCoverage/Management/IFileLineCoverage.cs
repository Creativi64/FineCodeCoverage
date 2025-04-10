using System.Collections.Generic;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal interface IFileLineCoverage
    {
        List<ICoberturaLine> GetLines(string filePath);
    }
}
