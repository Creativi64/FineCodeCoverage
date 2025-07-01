using System.Collections.Generic;
using System.Threading.Tasks;
using FineCodeCoverage.Collection.CoverageProjectManagement;

namespace FineCodeCoverage.Collection.CoverageToolOutput
{
    public interface ICoverageToolOutputManager
    {
        Task SetProjectCoverageOutputFolderAsync(List<ICoverageProject> coverageProjects);

        string GetReportOutputFolder();
    }
}
