using PalmmediaMetric = Palmmedia.ReportGenerator.Core.Parser.Analysis.Metric;

namespace FineCodeCoverage.Engine.ReportGenerator
{
    internal static class MetricNames
    {
        private static string crap;
        public static string Crap
        {
            get
            {
                if (crap == null)
                {
                    crap = PalmmediaMetric.CrapScore(0).Name;
                }

                return crap;
            }
        }
        private static string nPath;
        public static string NPath
        {
            get
            {
                if (nPath == null)
                {
                    nPath = PalmmediaMetric.NPathComplexity(0).Name;
                }

                return nPath;
            }
        }
        private static string cyclomaticComplexity;
        public static string CyclomaticComplexity
        {
            get
            {
                if (cyclomaticComplexity == null)
                {
                    cyclomaticComplexity = PalmmediaMetric.CyclomaticComplexity(0).Name;
                }

                return cyclomaticComplexity;
            }
        }
        private static string blocksCovered;
        public static string BlocksCovered
        {
            get
            {
                if (blocksCovered == null)
                {
                    blocksCovered = PalmmediaMetric.BlocksCovered(0).Name;
                }

                return blocksCovered;
            }
        }
        private static string blocksNotCovered;
        public static string BlocksNotCovered
        {
            get
            {
                if (blocksNotCovered == null)
                {
                    blocksNotCovered = PalmmediaMetric.BlocksNotCovered(0).Name;
                }

                return blocksNotCovered;
            }
        }
    }
}