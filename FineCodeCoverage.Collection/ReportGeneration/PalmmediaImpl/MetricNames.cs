using PalmmediaMetric = Palmmedia.ReportGenerator.Core.Parser.Analysis.Metric;

namespace FineCodeCoverage.Collection.ReportGeneration.PalmmediaImpl
{
    internal static class MetricNames
    {
        private static string s_crap;
        private static string s_nPath;
        private static string s_cyclomaticComplexity;
        private static string s_blocksCovered;
        private static string s_blocksNotCovered;

        public static string Crap
        {
            get
            {
                if (s_crap == null)
                {
                    s_crap = PalmmediaMetric.CrapScore(0).Name;
                }

                return s_crap;
            }
        }

        public static string NPath
        {
            get
            {
                if (s_nPath == null)
                {
                    s_nPath = PalmmediaMetric.NPathComplexity(0).Name;
                }

                return s_nPath;
            }
        }

        public static string CyclomaticComplexity
        {
            get
            {
                if (s_cyclomaticComplexity == null)
                {
                    s_cyclomaticComplexity = PalmmediaMetric.CyclomaticComplexity(0).Name;
                }

                return s_cyclomaticComplexity;
            }
        }

        public static string BlocksCovered
        {
            get
            {
                if (s_blocksCovered == null)
                {
                    s_blocksCovered = PalmmediaMetric.BlocksCovered(0).Name;
                }

                return s_blocksCovered;
            }
        }

        public static string BlocksNotCovered
        {
            get
            {
                if (s_blocksNotCovered == null)
                {
                    s_blocksNotCovered = PalmmediaMetric.BlocksNotCovered(0).Name;
                }

                return s_blocksNotCovered;
            }
        }
    }
}
