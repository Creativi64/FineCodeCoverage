using System.Collections.Generic;
using System.Threading.Tasks;
using FineCodeCoverage.Collection.CoverageProjectManagement.ReferencedProjects;
using VSLangProj;

namespace FineCodeCoverage.Engine.Model
{
    internal interface IDotNetReferencedProjectsHelper
    {
        Task<List<IExcludableReferencedProject>> GetReferencedProjectsAsync(VSProject vsProject);
    }
}
