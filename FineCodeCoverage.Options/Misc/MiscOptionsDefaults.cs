using System.ComponentModel.Composition;
using FineCodeCoverage.Options.Base;

namespace FineCodeCoverage.Options.Misc
{
    [Export(typeof(IDefaultOptionsSetter<MiscOptions>))]
    internal sealed class MiscOptionsDefaults : IDefaultOptionsSetter<MiscOptions>
    {
        public void Set(MiscOptions options)
        {
        }
    }
}
