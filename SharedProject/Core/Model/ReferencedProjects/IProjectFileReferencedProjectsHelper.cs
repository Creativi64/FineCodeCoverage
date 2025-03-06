using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FineCodeCoverage.Engine.Model
{
    interface IProjectFileReferencedProjectsHelper
    {
        Task<List<IExcludableReferencedProject>> GetReferencedProjectsAsync(string projectFile, XElement projectFileXElement);
    }
}
