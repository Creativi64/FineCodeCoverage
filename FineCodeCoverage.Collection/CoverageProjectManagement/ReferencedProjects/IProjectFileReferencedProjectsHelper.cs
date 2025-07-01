using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FineCodeCoverage.Collection.CoverageProjectManagement.ReferencedProjects
{
    internal interface IProjectFileReferencedProjectsHelper
    {
        Task<List<IExcludableReferencedProject>> GetReferencedProjectsAsync(string projectFile, XElement projectFileXElement);
    }
}
