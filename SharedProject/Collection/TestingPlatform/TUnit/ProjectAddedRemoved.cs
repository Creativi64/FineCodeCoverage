using Microsoft.VisualStudio.Shell.Interop;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    internal sealed class ProjectAddedRemoved
    {
        public ProjectAddedRemoved(bool added, IVsHierarchy project)
        {
            Added = added;
            Project = project;
        }

        public bool Added { get; }

        public IVsHierarchy Project { get; }
    }
}
