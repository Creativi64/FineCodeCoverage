using System.Collections.Generic;
using FineCodeCoverage.Collection.CoverageProjectManagement;

namespace FineCodeCoverage.Engine.OpenCover
{
    internal interface IOpenCoverExeArgumentsProvider
    {
        List<string> Provide(ICoverageProject coverageProject, string msTestPlatformExePath);
    }
}
