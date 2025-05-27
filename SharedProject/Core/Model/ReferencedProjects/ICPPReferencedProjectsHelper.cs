using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.VCProjectEngine;

namespace FineCodeCoverage.Engine.Model
{
    internal interface ICPPReferencedProjectsHelper
    {
        Task<List<IExcludableReferencedProject>> GetInstrumentableReferencedProjectsAsync(VCProject cppProject);
    }

}
