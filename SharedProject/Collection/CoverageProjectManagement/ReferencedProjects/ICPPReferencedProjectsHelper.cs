using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.VCProjectEngine;

namespace FineCodeCoverage.Collection.CoverageProjectManagement.ReferencedProjects
{
    internal interface ICPPReferencedProjectsHelper
    {
        Task<List<IExcludableReferencedProject>> GetInstrumentableReferencedProjectsAsync(VCProject cppProject);
    }
}
