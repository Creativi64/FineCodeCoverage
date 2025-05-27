using System.Collections.Generic;
using System.Threading.Tasks;
using FineCodeCoverage.Engine.Model;

namespace FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage
{
    internal interface ITemplatedRunSettingsService
    {
        Task<IProjectRunSettingsFromTemplateResult> GenerateAsync(IEnumerable<ICoverageProject> coverageProjectsWithoutRunSettings, string solutionDirectory, string fccMsTestAdapterPath);
        Task CleanUpAsync(List<ICoverageProject> coverageProjects);
        List<TemplatedCoverageProjectRunSettingsResult> CreateProjectsRunSettings(
            IEnumerable<ICoverageProject> coverageProjects,
            string solutionDirectory,
            string fccMsTestAdapterPath
        );
    }
}
