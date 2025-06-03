using FineCodeCoverage.Engine.ReportGenerator;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal interface IDynamicCodeElement : ICodeElement
    {
        void Deleted();
        void IsDirty();
    }
}