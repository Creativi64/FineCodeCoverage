namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal interface IDynamicLine
    {
        int OriginalLineNumber { get; }

        int LineNumber { get; }

        DynamicCoverageType CoverageType { get; }
    }
}
