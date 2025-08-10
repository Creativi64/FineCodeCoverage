using FineCodeCoverage.Editor.DynamicCoverage.Common;

namespace FineCodeCoverage.Editor.Management
{
    internal interface ICoverageColours
    {
        IItemCoverageColours GetColour(DynamicCoverageType coverageType);
    }
}
