using FineCodeCoverage.Engine.ReportGenerator;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    public interface IDynamicCodeElement : ICodeElement
    {
        void Deleted();

        void IsDirty();
    }
}
