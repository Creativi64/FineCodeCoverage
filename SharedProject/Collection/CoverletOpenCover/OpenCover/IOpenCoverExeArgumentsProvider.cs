using System.Collections.Generic;
using FineCodeCoverage.Engine.Model;

namespace FineCodeCoverage.Engine.OpenCover
{
    internal interface IOpenCoverExeArgumentsProvider
    {
        List<string> Provide(ICoverageProject coverageProject, string msTestPlatformExePath);
    }
}
