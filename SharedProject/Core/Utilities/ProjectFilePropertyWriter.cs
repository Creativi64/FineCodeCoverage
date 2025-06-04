using System.ComponentModel.Composition;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace FineCodeCoverage.Core.Utilities
{
    [Export(typeof(IProjectFilePropertyWriter))]
    public class ProjectFilePropertyWriter : IProjectFilePropertyWriter
    {
        public async System.Threading.Tasks.Task<bool> WritePropertyAsync(IVsHierarchy projectHierarchy, string propertyName, string value)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (!(projectHierarchy is IVsBuildPropertyStorage vsBuildPropertyStorage))
            {
                return false;
            }

            int result = vsBuildPropertyStorage.GetPropertyValue(propertyName, string.Empty, (uint)_PersistStorageType.PST_PROJECT_FILE, out string v);
            return (result == VSConstants.S_OK && v == value) || vsBuildPropertyStorage.SetPropertyValue(propertyName, string.Empty, (uint)_PersistStorageType.PST_PROJECT_FILE, value) == VSConstants.S_OK;
        }

        public async System.Threading.Tasks.Task<bool> RemovePropertyAsync(IVsHierarchy pHierProj, string propertyName)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            return pHierProj is IVsBuildPropertyStorage vsBuildPropertyStorage &&
                vsBuildPropertyStorage.RemoveProperty(
                    propertyName,
                    string.Empty,
                    (uint)_PersistStorageType.PST_PROJECT_FILE
                ) == VSConstants.S_OK;
        }
    }
}
