using System.ComponentModel.Composition;
using FineCodeCoverage.Options.Base;

namespace FineCodeCoverage.Options.OpenCover
{
    [Export(typeof(IDefaultOptionsSetter<OpenCoverOptions>))]
    internal sealed class OpenCoverOptionsDefaults : IDefaultOptionsSetter<OpenCoverOptions>
    {
        public void Set(OpenCoverOptions options)
        {
        }
    }
}
