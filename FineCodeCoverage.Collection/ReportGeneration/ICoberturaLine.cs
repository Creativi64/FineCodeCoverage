namespace FineCodeCoverage.Collection.ReportGeneration
{
    public interface ICoberturaLine
    {
        int Number { get; }

        CoverageType CoverageType { get; }
    }
}
