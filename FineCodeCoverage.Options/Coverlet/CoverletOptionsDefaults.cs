using System.ComponentModel.Composition;
using FineCodeCoverage.Options.Base;

namespace FineCodeCoverage.Options.Coverlet
{
    [Export(typeof(IDefaultOptionsSetter<CoverletOptions>))]
    internal sealed class CoverletOptionsDefaults : IDefaultOptionsSetter<CoverletOptions>
    {
        public void Set(CoverletOptions options) => options.RunSettingsOnly = true;
    }
}
