namespace FineCodeCoverage.Engine.Model
{
    internal interface ICoberturaLine
    {
        int Number { get; }
        CoverageType CoverageType { get; }
    }
}
