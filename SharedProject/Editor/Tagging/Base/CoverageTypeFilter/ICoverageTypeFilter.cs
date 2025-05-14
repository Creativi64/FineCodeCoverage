using FineCodeCoverage.Editor.DynamicCoverage;
using FineCodeCoverage.Options;

namespace FineCodeCoverage.Editor.Tagging.Base
{
    internal interface ICoverageTypeFilter
    {
        void Initialize(AppOptions appOptions);
        bool Disabled { get; }
        bool Show(DynamicCoverageType coverageType);
        string TypeIdentifier { get; }
        bool Changed(ICoverageTypeFilter other);
    }
}
