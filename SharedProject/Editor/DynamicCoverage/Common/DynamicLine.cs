using FineCodeCoverage.Engine.ReportGenerator;

namespace FineCodeCoverage.Editor.DynamicCoverage
{
    internal class DynamicLine : IDynamicLine
    {
        public DynamicLine(int number, DynamicCoverageType coverageType)
        {
            OriginalLineNumber = number;
            LineNumber = number;
            CoverageType = coverageType;
        }

        public static DynamicLine FromCoberturaLine(ICoberturaLine coberturaLine)
            => new DynamicLine(coberturaLine.Number - 1, DynamicCoverageTypeConverter.Convert(coberturaLine.CoverageType));

        public int OriginalLineNumber { get; }

        public int LineNumber { get; set; }

        public DynamicCoverageType CoverageType { get; }
    }
}
