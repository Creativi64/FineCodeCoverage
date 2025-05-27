using System.Collections.Generic;
using FineCodeCoverage.Engine.Model;

namespace FineCodeCoverage.Engine.Coverlet
{
    internal interface ICoverletExeArgumentsProvider
    {
        List<string> GetArguments(ICoverageProject project);
    }
}
