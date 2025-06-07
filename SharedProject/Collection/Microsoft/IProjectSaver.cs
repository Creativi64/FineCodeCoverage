using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell.Interop;

namespace FineCodeCoverage.Core.MsTestPlatform.CodeCoverage
{
    internal interface IProjectSaver
    {
        Task SaveProjectAsync(IVsHierarchy projectHierarchy);
    }
}
