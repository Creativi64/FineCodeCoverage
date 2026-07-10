using System.ComponentModel.Composition;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace FineCodeCoverage.Utilities.Files
{
    [Export(typeof(IProjectFilePropertyWriter))]
    public class ProjectFilePropertyWriter : IProjectFilePropertyWriter
    {
        public async System.Threading.Tasks.Task<bool> RemovePropertyAsync(IVsHierarchy pHierProj, string propertyName)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            return pHierProj is IVsBuildPropertyStorage vsBuildPropertyStorage &&
                vsBuildPropertyStorage.RemoveProperty(
                    propertyName,
                    string.Empty,
                    (uint)_PersistStorageType.PST_PROJECT_FILE) == VSConstants.S_OK;
        }
    }
}
