using System.Collections.Generic;
using FineCodeCoverage.Engine.ReportGenerator;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal interface IFileLineCoverage
    {
        List<ICoberturaLine> GetLines(string filePath);
    }
}
