namespace FineCodeCoverage.Engine.ReportGenerator
{
    public interface ICoberturaLine
    {
        int Number { get; }
        CoverageType CoverageType { get; }
    }
}