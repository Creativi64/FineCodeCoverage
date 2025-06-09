namespace FineCodeCoverage.Editor.DynamicCoverage
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
