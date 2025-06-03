using System.Collections.Generic;
using System.Threading.Tasks;
using FineCodeCoverage.Engine.Model;

namespace FineCodeCoverage.Engine
{
    internal interface ICoverageToolOutputManager
    {
        Task SetProjectCoverageOutputFolderAsync(List<ICoverageProject> coverageProjects);
        string GetReportOutputFolder();
    }
}