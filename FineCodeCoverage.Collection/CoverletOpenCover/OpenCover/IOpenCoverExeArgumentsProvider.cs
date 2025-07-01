using System.Collections.Generic;
using FineCodeCoverage.Collection.CoverageProjectManagement;

namespace FineCodeCoverage.Collection.CoverletOpenCover.OpenCover
{
    internal interface IOpenCoverExeArgumentsProvider
    {
        List<string> Provide(ICoverageProject coverageProject, string msTestPlatformExePath);
    }
}
