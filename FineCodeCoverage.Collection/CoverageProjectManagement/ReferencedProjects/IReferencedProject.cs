namespace FineCodeCoverage.Collection.CoverageProjectManagement.ReferencedProjects
{
    public interface IReferencedProject
    {
        string AssemblyName { get; }

        bool IsDll { get; }
    }
}
