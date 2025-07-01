using System.ComponentModel.Composition;

namespace FineCodeCoverage.Options
{
    [Export(typeof(IDefaultOptionsSetter<HotspotThresholdsOptions>))]
    internal sealed class HotspotThresholdsOptionsDefaults : IDefaultOptionsSetter<HotspotThresholdsOptions>
    {
        public void Set(HotspotThresholdsOptions options)
        {
            options.ThresholdForCrapScore = 15;
            options.ThresholdForNPathComplexity = 200;
            options.ThresholdForCyclomaticComplexity = 30;
        }
    }
}
