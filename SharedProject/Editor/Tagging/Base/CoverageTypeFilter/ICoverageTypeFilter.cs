using FineCodeCoverage.Editor.DynamicCoverage;
using FineCodeCoverage.Options.EditorCoverageColouring;

namespace FineCodeCoverage.Editor.Tagging.Base
{
    internal interface ICoverageTypeFilter
    {
        void Initialize(EditorCoverageColouringOptions editorCoverageColouringOptions);

        bool Disabled { get; }

        bool Show(DynamicCoverageType coverageType);

        string TypeIdentifier { get; }

        bool Changed(ICoverageTypeFilter other);
    }
}
