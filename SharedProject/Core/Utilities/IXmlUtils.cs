using System.Xml.Linq;

namespace FineCodeCoverage.Core.Utilities
{
    internal interface IXmlUtils
    {
        XElement Load(string path);

        string Serialize(XElement xmlElement);
    }
}
