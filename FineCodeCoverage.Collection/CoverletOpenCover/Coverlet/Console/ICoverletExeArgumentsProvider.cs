using System.Collections.Generic;
using FineCodeCoverage.Collection.CoverageProjectManagement;

namespace FineCodeCoverage.Engine.Coverlet
{
    internal interface ICoverletExeArgumentsProvider
    {
        List<string> GetArguments(ICoverageProject project);
    }
}
