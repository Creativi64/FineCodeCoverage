using System.Collections.Generic;

namespace FineCodeCoverage.Utilities.DotNetToolList
{
    public interface IDotNetToolListParser
    {
        List<DotNetToolInfo> Parse(string output);
    }
}
