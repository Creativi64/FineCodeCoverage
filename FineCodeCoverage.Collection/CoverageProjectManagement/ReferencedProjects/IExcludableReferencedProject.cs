namespace FineCodeCoverage.Engine.Model
{
    public interface IExcludableReferencedProject : IReferencedProject
    {
        bool ExcludeFromCodeCoverage { get; }
    }
}
