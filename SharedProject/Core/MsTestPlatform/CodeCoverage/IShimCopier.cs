using System.Collections.Generic;
using FineCodeCoverage.Engine.Model;

namespace FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage
{
    internal interface IShimCopier
    {
        void Copy(string shimPath, IEnumerable<ICoverageProject> coverageProjects);
    }
}