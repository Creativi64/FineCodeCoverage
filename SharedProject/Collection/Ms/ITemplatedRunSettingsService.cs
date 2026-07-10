using System.Collections.Generic;
using System.Threading.Tasks;
using FineCodeCoverage.Collection.CoverageProjectManagement;

namespace FineCodeCoverage.Collection.Ms
{
    internal interface ITemplatedRunSettingsService
    {
        Task CleanUpAsync(List<ICoverageProject> coverageProjects);

        List<TemplatedCoverageProjectRunSettingsResult> CreateProjectsRunSettings(
            IEnumerable<ICoverageProject> coverageProjects,
            string solutionDirectory,
            string fccMsTestAdapterPath);
    }
}
