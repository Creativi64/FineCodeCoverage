using System.Xml.Linq;

namespace FineCodeCoverage.Core.Utilities
{
    public interface IXmlUtils
    {
        XElement Load(string path);

        string Serialize(XElement xmlElement);
    }
}
