using System.ComponentModel.Composition;

namespace FineCodeCoverage.Options
{
    [Export(typeof(IDefaultOptionsSetter<OpenCoverOptions>))]
    internal sealed class OpenCoverOptionsDefaults : IDefaultOptionsSetter<OpenCoverOptions>
    {
        public void Set(OpenCoverOptions options)
        {
        }
    }
}
