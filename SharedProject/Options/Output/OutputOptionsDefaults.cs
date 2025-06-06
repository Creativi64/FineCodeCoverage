using System.ComponentModel.Composition;

namespace FineCodeCoverage.Options
{
    [Export(typeof(IDefaultOptionsSetter<OutputOptions>))]
    internal sealed class OutputOptionsDefaults : IDefaultOptionsSetter<OutputOptions>
    {
        public void Set(OutputOptions options)
        {
        }
    }
}
