namespace FineCodeCoverage.Collection.ReportGeneration
{
    public interface IDynamicCoberturaLine : ICoberturaLine
    {
        IDynamicCodeElement CodeElement { get; }

        void LineMoved(int newLineNumber);
    }
}
