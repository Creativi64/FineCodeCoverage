using System.ComponentModel.Composition;
using System.Net;

namespace FineCodeCoverage.Core.Utilities
{
    [Export(typeof(IUrlEncoder))]
    internal class UrlEncoder : IUrlEncoder
    {
        public string Encode(string url) => WebUtility.UrlEncode(url);
    }
}
