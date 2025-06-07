using System;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Properties;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    internal static class IVsHierarchyExtensions
    {
        // https://github.com/microsoft/VSProjectSystem/blob/master/doc/automation/finding_CPS_in_a_VS_project.md
        public static async Task<UnconfiguredProject> AsUnconfiguredProjectAsync(this IVsHierarchy hier)
        {
            if (hier is IVsBrowseObjectContext context)
            {
                return context.UnconfiguredProject;
            }
            else
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                if (ErrorHandler.Succeeded(hier.GetProperty(4294967294U, -2027, out object pvar)) && pvar is Project project)
                {
                    context = project.Object as IVsBrowseObjectContext;
                    return context.UnconfiguredProject;
                }
            }

            return null;
        }

        public static Project ToProject(this IVsHierarchy hierarchy)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // Retrieve the automation object from the root of the hierarchy.
            int hr = hierarchy.GetProperty(
                VSConstants.VSITEMID_ROOT,
                (int)__VSHPROPID.VSHPROPID_ExtObject,
                out object extObject);

            return ErrorHandler.Succeeded(hr) && extObject is Project project ? project : null;
        }

        public static Guid GetGuid(this IVsHierarchy hierarchy)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _ = hierarchy.GetGuidProperty(
                VSConstants.VSITEMID_ROOT,
                (int)__VSHPROPID.VSHPROPID_ProjectIDGuid,
                out Guid projectGuid);

            return projectGuid;
        }

        public static async Task<Guid> GetGuidAsync(this IVsHierarchy hierarchy)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            return GetGuid(hierarchy);
        }
    }
}
