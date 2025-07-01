using System.ComponentModel.Composition;
using FineCodeCoverage.Options.Base;

namespace FineCodeCoverage.Options.Output
{
    [Export(typeof(IDefaultOptionsSetter<OutputOptions>))]
    internal sealed class OutputOptionsDefaults : IDefaultOptionsSetter<OutputOptions>
    {
        public void Set(OutputOptions options)
        {
        }
    }
}
