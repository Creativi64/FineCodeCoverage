using FineCodeCoverage.Engine.ReportGenerator;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    public interface IDynamicCoberturaLine : ICoberturaLine
    {
        IDynamicCodeElement CodeElement { get; }

        void LineMoved(int newLineNumber);
    }
}
