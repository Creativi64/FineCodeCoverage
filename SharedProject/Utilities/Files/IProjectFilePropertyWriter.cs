using Microsoft.VisualStudio.Shell.Interop;

namespace FineCodeCoverage.Utilities.Files
{
    public interface IProjectFilePropertyWriter
    {
        System.Threading.Tasks.Task<bool> RemovePropertyAsync(IVsHierarchy pHierProj, string propertyName);
    }
}
