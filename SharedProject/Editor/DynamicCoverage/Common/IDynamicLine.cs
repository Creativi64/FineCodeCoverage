namespace FineCodeCoverage.Editor.DynamicCoverage.Common
{
    internal interface IDynamicLine
    {
        int OriginalLineNumber { get; }

        int LineNumber { get; }

        DynamicCoverageType CoverageType { get; }
    }
}
