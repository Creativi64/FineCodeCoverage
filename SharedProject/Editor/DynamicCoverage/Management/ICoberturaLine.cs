namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal interface ICoberturaLine
    {
        int Number { get; }
        CoverageType CoverageType { get; }
    }
}
