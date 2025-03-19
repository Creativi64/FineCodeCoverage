using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    public static class ProjectDependencyHelper
    {
        /// <summary>
        /// Gets the transitive closure of project dependencies.
        /// </summary>
        /// <param name="buildManager">The IVsSolutionBuildManager2 instance.</param>
        /// <param name="projects">The projects for which dependencies are required.</param>
        /// <returns>A list of IVsHierarchy instances representing all transitive dependencies.</returns>
        public static List<IVsHierarchy> GetTransitiveDependencies(IVsSolutionBuildManager2 buildManager, IEnumerable<IVsHierarchy> projects)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // Ensure the dependency information is calculated.
            int hr = buildManager.CalculateProjectDependencies();
            ErrorHandler.ThrowOnFailure(hr);

            var collected = new HashSet<IVsHierarchy>(new VsHierarchyComparer());
            foreach (var project in projects)
            {
                AddDependencies(buildManager, project, collected);
            }
            return collected.ToList();
        }

        /// <summary>
        /// Recursively adds direct dependencies to the collected set.
        /// </summary>
        private static void AddDependencies(IVsSolutionBuildManager2 buildManager, IVsHierarchy project, HashSet<IVsHierarchy> collected)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            // First call: Get the count of direct dependencies.
            uint[] actualCount = new uint[1];
            int hr = buildManager.GetProjectDependencies(project, 0, null, actualCount);
            ErrorHandler.ThrowOnFailure(hr);
            uint dependencyCount = actualCount[0];

            if (dependencyCount == 0)
            {
                return;
            }

            // Allocate an array for the dependencies and call again to fill it.
            IVsHierarchy[] directDependencies = new IVsHierarchy[dependencyCount];
            uint[] actualCount2 = new uint[1];
            hr = buildManager.GetProjectDependencies(project, dependencyCount, directDependencies, actualCount2);
            ErrorHandler.ThrowOnFailure(hr);

            // Process each direct dependency recursively.
            foreach (var dep in directDependencies)
            {
                if (dep != null && collected.Add(dep))
                {
                    AddDependencies(buildManager, dep, collected);
                }
            }
        }
    }

    /// <summary>
    /// Compares IVsHierarchy objects by comparing their project GUIDs.
    /// </summary>
    public class VsHierarchyComparer : IEqualityComparer<IVsHierarchy>
    {
        public bool Equals(IVsHierarchy x, IVsHierarchy y)
        {
            if (x == null || y == null)
            {
                return false;
            }
            return x.GetGuid().Equals(y.GetGuid());
        }

        public int GetHashCode(IVsHierarchy obj)
        {
            if (obj == null)
            {
                return 0;
            }
            return obj.GetGuid().GetHashCode();
        }
    }

}
