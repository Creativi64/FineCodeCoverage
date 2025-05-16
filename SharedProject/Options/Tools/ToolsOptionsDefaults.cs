using System.ComponentModel.Composition;

namespace FineCodeCoverage.Options.Tools
{
    [Export(typeof(IDefaultOptionsSetter<ToolsOptions>))]
    internal class ToolsOptionsDefaults : IDefaultOptionsSetter<ToolsOptions>
    {
        public void Set(ToolsOptions options) { }
    }
}
