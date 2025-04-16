namespace FineCodeCoverage.Engine.ReportGenerator
{
    public enum CoverageType { Covered, Partial, NotCovered }
    public interface ICoberturaLine
    {
        int Number { get; }
        CoverageType CoverageType { get; }
    }
}
