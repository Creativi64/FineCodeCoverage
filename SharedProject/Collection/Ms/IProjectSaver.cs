using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell.Interop;

namespace FineCodeCoverage.Collection.Ms
{
    internal interface IProjectSaver
    {
        Task SaveProjectAsync(IVsHierarchy projectHierarchy);
    }
}
