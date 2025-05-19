using System.ComponentModel.Composition;

namespace FineCodeCoverage.Options.Tools
{
    [Export(typeof(IDefaultOptionsSetter<MiscOptions>))]
    internal class MiscOptionsDefaults : IDefaultOptionsSetter<MiscOptions>
    {
        public void Set(MiscOptions options) { }
    }
}
