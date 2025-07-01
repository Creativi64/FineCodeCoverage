namespace FineCodeCoverage.Collection.CoverageProjectManagement.ReferencedProjects
{
    public interface IExcludableReferencedProject : IReferencedProject
    {
        bool ExcludeFromCodeCoverage { get; }
    }
}
