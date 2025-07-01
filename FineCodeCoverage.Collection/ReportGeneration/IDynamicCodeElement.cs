namespace FineCodeCoverage.Collection.ReportGeneration
{
    public interface IDynamicCodeElement : ICodeElement
    {
        void Deleted();

        void IsDirty();
    }
}
