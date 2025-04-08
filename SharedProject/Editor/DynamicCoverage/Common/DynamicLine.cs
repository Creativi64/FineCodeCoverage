using FineCodeCoverage.Engine.Model;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal class DynamicLine : IDynamicLine
    {
        // ! Constructor parameters should match property name for serialization
        public DynamicLine(int number, DynamicCoverageType coverageType)
        {
            this.Number = number;
            this.CoverageType = coverageType;
        }

        public static DynamicLine FromCoberturaLine(ICoberturaLine coberturaLine)
            => new DynamicLine(coberturaLine.Number - 1, DynamicCoverageTypeConverter.Convert(coberturaLine.CoverageType));

        public int Number { get; set; }

        public DynamicCoverageType CoverageType { get; }
    }
}
