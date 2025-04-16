using FineCodeCoverage.Engine.ReportGenerator;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal interface IDynamicCoberturaLine : ICoberturaLine
    {
        void LineMoved(int newLineNumber);
    }
}
