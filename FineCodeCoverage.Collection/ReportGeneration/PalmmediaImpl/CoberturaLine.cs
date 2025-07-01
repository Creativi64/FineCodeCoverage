namespace FineCodeCoverage.Collection.ReportGeneration.PalmmediaImpl
{
    internal sealed class CoberturaLine : ICoberturaLine
    {
        public CoberturaLine(int number, CoverageType coverageType)
        {
            Number = number;
            CoverageType = coverageType;
        }

        public int Number { get; }

        public CoverageType CoverageType { get; }
    }
}
