namespace FineCodeCoverage.Editor.DynamicCoverage.Common
{
    internal enum DynamicCoverageType
    {
        // Coverage type
        Covered,
        Partial,
        NotCovered,

        // dynamic specific
        Dirty,
        NewLine,
        NotIncluded,
    }
}
