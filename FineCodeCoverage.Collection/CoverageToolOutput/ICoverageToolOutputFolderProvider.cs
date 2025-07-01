using System.Collections.Generic;
using FineCodeCoverage.Collection.CoverageProjectManagement;

namespace FineCodeCoverage.Collection.CoverageToolOutput
{
    public interface ICoverageToolOutputFolderProvider
    {
        string Provide(List<ICoverageProject> coverageProjects);
    }
}
