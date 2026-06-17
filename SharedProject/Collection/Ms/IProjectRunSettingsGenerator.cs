using System.Collections.Generic;
using System.Threading.Tasks;
using FineCodeCoverage.Collection.CoverageProjectManagement;

namespace FineCodeCoverage.Collection.Ms
{
    internal interface IProjectRunSettingsGenerator
    {
        Task RemoveGeneratedProjectSettingsAsync(IEnumerable<ICoverageProject> coverageProjects);
    }
}
