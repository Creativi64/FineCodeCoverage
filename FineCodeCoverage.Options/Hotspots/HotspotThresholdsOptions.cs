using System.ComponentModel;
using FineCodeCoverage.Options.Base;

namespace FineCodeCoverage.Options.Hotspots
{
    public sealed class HotspotThresholdsOptions
    {
        private const string OpenCoverCategory = "OpenCover";

        [Category(CommonCategories.Common)]
        [Description("When cyclomatic complexity exceeds this value for a method then the method will be present in the risk hotspots tab.")]
        [DisplayName("Cyclomatic Complexity")]

        public int ThresholdForCyclomaticComplexity { get; set; }

        [Category(OpenCoverCategory)]
        [Description("When npath complexity exceeds this value for a method then the method will be present in the risk hotspots tab. OpenCover only")]
        [DisplayName("NPath Complexity")]
        public int ThresholdForNPathComplexity { get; set; }

        [Category(OpenCoverCategory)]
        [Description("When crap score exceeds this value for a method then the method will be present in the risk hotspots tab. OpenCover only")]
        [DisplayName("Crap Score")]
        public int ThresholdForCrapScore { get; set; }
    }
}
