using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Xml.Linq;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    [Export(typeof(IRunSettingsToConfiguration))]
    internal sealed class RunSettingsToConfiguration : IRunSettingsToConfiguration
    {
        public XElement ConvertToConfiguration(XElement runSettingsElement)
        {
            XElement dataCollectorsElement = runSettingsElement.Element("DataCollectionRunSettings").Element("DataCollectors");
            XElement codeCoverageDataCollectorElement = dataCollectorsElement.Elements().FirstOrDefault(dataCollectorElement =>
            {
                string friendlyName = dataCollectorElement.Attribute((XName)"friendlyName")?.Value ?? string.Empty;
                return friendlyName.Equals("Code Coverage", StringComparison.OrdinalIgnoreCase);
            });
            return codeCoverageDataCollectorElement?.Element("Configuration");
        }
    }
}
