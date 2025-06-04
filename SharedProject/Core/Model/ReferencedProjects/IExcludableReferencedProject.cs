namespace FineCodeCoverage.Engine.Model
{
    internal interface IExcludableReferencedProject : IReferencedProject
    {
        bool ExcludeFromCodeCoverage { get; }
    }
}
