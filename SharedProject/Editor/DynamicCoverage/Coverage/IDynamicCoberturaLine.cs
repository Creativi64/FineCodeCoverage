using FineCodeCoverage.Engine.ReportGenerator;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal interface IDynamicCoberturaLine : ICoberturaLine
    {
        IDynamicCodeElement CodeElement { get; }
        void LineMoved(int newLineNumber);
    }
}