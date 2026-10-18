using FineCodeCoverage.Collection.CoverageProjectManagement;

namespace FineCodeCoverage.Collection.Engine
{
    public enum ProjectCoverageState
    {
        Collected,
        Skipped,
        Unknown,
    }

    public sealed class ProjectCoverageResult
    {
        public ProjectCoverageResult(ICoverageProject project, ProjectCoverageState state, string reason)
        {
            Project = project;
            State = state;
            Reason = reason;
        }

        public ICoverageProject Project { get; }

        public ProjectCoverageState State { get; }

        public string Reason { get; }
    }
}
