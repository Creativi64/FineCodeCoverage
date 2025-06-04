using System.ComponentModel.Composition;

namespace FineCodeCoverage.Options.Hotspots
{
    [Export(typeof(IDefaultOptionsSetter<OpenCoverOptions>))]
    internal class OpenCoverOptionsDefaults : IDefaultOptionsSetter<OpenCoverOptions>
    {
        public void Set(OpenCoverOptions options)
        {
        }
    }
}
