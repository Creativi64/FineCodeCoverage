using System.Collections.Generic;
using FineCodeCoverage.Collection.CoverageProjectManagement;

namespace FineCodeCoverage.Collection.CoverletOpenCover.Coverlet.Console
{
    internal interface ICoverletExeArgumentsProvider
    {
        List<string> GetArguments(ICoverageProject project);
    }
}
