using System.Collections.Generic;
using System.Threading.Tasks;

namespace FineCodeCoverage.Collection.CoverageProjectManagement.ReferencedProjects
{
    public interface IVsApiReferencedProjectsHelper
    {
        Task<List<IExcludableReferencedProject>> GetReferencedProjectsAsync(string projectFile);
    }
}
