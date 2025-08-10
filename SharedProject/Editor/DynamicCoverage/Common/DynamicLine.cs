using FineCodeCoverage.Collection.ReportGeneration;

namespace FineCodeCoverage.Editor.DynamicCoverage.Common
{
    internal sealed class DynamicLine : IDynamicLine
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
