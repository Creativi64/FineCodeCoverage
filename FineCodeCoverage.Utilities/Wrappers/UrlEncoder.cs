using System.ComponentModel.Composition;
using System.Net;

namespace FineCodeCoverage.Utilities.Wrappers
{
    [Export(typeof(IUrlEncoder))]
    internal sealed class UrlEncoder : IUrlEncoder
    {
        public string Encode(string url) => WebUtility.UrlEncode(url);
    }
}
