using FineCodeCoverage.Collection.ReportGeneration;
using FineCodeCoverage.Editor.DynamicCoverage.Management;

namespace FineCodeCoverage.Editor.DynamicCoverage.Common
{
    internal static class DynamicCoverageTypeConverter
    {
        public static DynamicCoverageType Convert(CoverageType coverageType)
        {
            switch (coverageType)
            {
                case CoverageType.NotCovered:
                    return DynamicCoverageType.NotCovered;
                case CoverageType.Partial:
                    return DynamicCoverageType.Partial;
            }

            return DynamicCoverageType.Covered;
        }

        public static CoverageType Convert(DynamicCoverageType coverageType)
        {
            switch (coverageType)
            {
                case DynamicCoverageType.NotCovered:
                    return CoverageType.NotCovered;
                case DynamicCoverageType.Partial:
                    return CoverageType.Partial;
            }

            return CoverageType.Covered;
        }
    }
}
