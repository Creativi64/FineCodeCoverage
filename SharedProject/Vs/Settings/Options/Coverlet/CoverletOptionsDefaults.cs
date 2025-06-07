using System.ComponentModel.Composition;

namespace FineCodeCoverage.Options.Hotspots
{
    [Export(typeof(IDefaultOptionsSetter<CoverletOptions>))]
    internal sealed class CoverletOptionsDefaults : IDefaultOptionsSetter<CoverletOptions>
    {
        public void Set(CoverletOptions options) => options.RunSettingsOnly = true;
    }
}
