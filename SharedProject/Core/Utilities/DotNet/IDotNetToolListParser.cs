using System.Collections.Generic;

namespace FineCodeCoverage.Core.Utilities
{
    internal interface IDotNetToolListParser
    {
        List<DotNetToolInfo> Parse(string output);
    }
}