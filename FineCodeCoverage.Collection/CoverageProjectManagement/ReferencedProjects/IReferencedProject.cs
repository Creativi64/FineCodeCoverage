namespace FineCodeCoverage.Engine.Model
{
    public interface IReferencedProject
    {
        string AssemblyName { get; }

        bool IsDll { get; }
    }
}
