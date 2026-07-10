using System.Xml.Linq;

namespace FineCodeCoverage.Utilities.Xml
{
    public interface IXmlUtils
    {
        XElement Load(string path);

        string Serialize(XElement xmlElement);
    }
}
