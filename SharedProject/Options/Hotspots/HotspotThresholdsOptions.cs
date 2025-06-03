using System.ComponentModel;

namespace FineCodeCoverage.Options
{
    internal class HotspotThresholdsOptions
    {
        private const string openCoverCategory = "OpenCover";
        private const string commonCategory = "Common";

        [Category(commonCategory)]
        [Description("When cyclomatic complexity exceeds this value for a method then the method will be present in the risk hotspots tab.")]
        [DisplayName("Cyclomatic Complexity")]

        public int ThresholdForCyclomaticComplexity { get; set; }
        [Category(openCoverCategory)]
        [Description("When npath complexity exceeds this value for a method then the method will be present in the risk hotspots tab. OpenCover only")]
        [DisplayName("NPath Complexity")]
        public int ThresholdForNPathComplexity { get; set; }

        [Category(openCoverCategory)]
        [Description("When crap score exceeds this value for a method then the method will be present in the risk hotspots tab. OpenCover only")]
        [DisplayName("Crap Score")]
        public int ThresholdForCrapScore { get; set; }
    }
}