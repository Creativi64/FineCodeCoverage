using System.Collections.Generic;
using System.Threading.Tasks;
using VSLangProj;

namespace FineCodeCoverage.Collection.CoverageProjectManagement.ReferencedProjects
{
    internal interface IDotNetReferencedProjectsHelper
    {
        Task<List<IExcludableReferencedProject>> GetReferencedProjectsAsync(VSProject vsProject);
    }
}
