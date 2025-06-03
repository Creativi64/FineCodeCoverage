using Microsoft.VisualStudio.Shell.Interop;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    internal class ProjectAddedRemoved
    {
        public ProjectAddedRemoved(bool added, IVsHierarchy project)
        {
            this.Added = added;
            this.Project = project;
        }
        public bool Added { get; }
        public IVsHierarchy Project { get; }
    }
}