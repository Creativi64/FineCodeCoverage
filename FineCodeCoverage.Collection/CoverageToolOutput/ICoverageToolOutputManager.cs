using System.Collections.Generic;
using System.Threading.Tasks;
using FineCodeCoverage.Engine.Model;

namespace FineCodeCoverage.Engine
{
    public interface ICoverageToolOutputManager
    {
        Task SetProjectCoverageOutputFolderAsync(List<ICoverageProject> coverageProjects);

        string GetReportOutputFolder();
    }
}
