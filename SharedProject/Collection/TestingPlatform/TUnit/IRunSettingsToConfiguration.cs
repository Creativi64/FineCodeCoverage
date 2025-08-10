using System.Xml.Linq;

namespace FineCodeCoverage.Collection.TestingPlatform.TUnit
{
    internal interface IRunSettingsToConfiguration
    {
        XElement ConvertToConfiguration(XElement runSettingsElement);
    }
}
