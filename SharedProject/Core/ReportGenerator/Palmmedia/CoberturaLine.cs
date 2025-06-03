namespace FineCodeCoverage.Engine.ReportGenerator
{
    internal class CoberturaLine : ICoberturaLine
    {
        public CoberturaLine(int number, CoverageType coverageType)
        {
            this.Number = number;
            this.CoverageType = coverageType;
        }
        public int Number { get; }
        public CoverageType CoverageType { get; }
    }
}