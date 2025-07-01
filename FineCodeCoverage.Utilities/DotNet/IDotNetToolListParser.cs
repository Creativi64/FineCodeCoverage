using System.Collections.Generic;

namespace FineCodeCoverage.Core.Utilities
{
    public interface IDotNetToolListParser
    {
        List<DotNetToolInfo> Parse(string output);
    }
}
