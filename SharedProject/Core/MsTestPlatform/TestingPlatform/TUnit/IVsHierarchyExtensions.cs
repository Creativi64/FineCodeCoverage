using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio;
using System;
using System.Threading.Tasks;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    internal static class IVsHierarchyExtensions
    {
        public static EnvDTE.Project ToProject(this IVsHierarchy hierarchy)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            // Retrieve the automation object from the root of the hierarchy.
            int hr = hierarchy.GetProperty(
                VSConstants.VSITEMID_ROOT,
                (int)__VSHPROPID.VSHPROPID_ExtObject,
                out object extObject);

            if (ErrorHandler.Succeeded(hr) && extObject is EnvDTE.Project project)
            {
                return project;
            }

            return null;
        }

        public static Guid GetGuid(this IVsHierarchy hierarchy)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            int hr = hierarchy.GetGuidProperty(
                VSConstants.VSITEMID_ROOT,
                (int)__VSHPROPID.VSHPROPID_ProjectIDGuid,
                out Guid projectGuid);

            return projectGuid;
        }

        public async static Task<Guid> GetGuidAsync(this IVsHierarchy hierarchy)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            return GetGuid(hierarchy);
        }
    }
}
