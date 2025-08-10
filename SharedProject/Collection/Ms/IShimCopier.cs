using System.Collections.Generic;
using FineCodeCoverage.Collection.CoverageProjectManagement;

namespace FineCodeCoverage.Collection.Ms
{
    internal interface IShimCopier
    {
        void Copy(string shimPath, IEnumerable<ICoverageProject> coverageProjects);
    }
}
