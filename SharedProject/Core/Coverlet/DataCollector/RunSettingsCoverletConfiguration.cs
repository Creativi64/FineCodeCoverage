using System.Linq;
using System.Xml.Linq;

namespace FineCodeCoverage.Core.Coverlet
{
    internal class RunSettingsCoverletConfiguration : IRunSettingsCoverletConfiguration
    {
        public bool Read(string runSettingsXml)
        {
            XDocument document = XDocument.Parse(runSettingsXml);
            //<DataCollector friendlyName=""XPlat code coverage"">
            XElement coverletDataCollectorElement = document.Descendants("DataCollector").FirstOrDefault(dataCollector =>
            {
                XAttribute friendlyNameAttribute = dataCollector.Attribute("friendlyName");
                return (friendlyNameAttribute == null ? "" : friendlyNameAttribute.Value) == "XPlat code coverage";
            });

            if (coverletDataCollectorElement != null)
            {
                XAttribute enabledAttribute = coverletDataCollectorElement.Attribute("enabled");
                if (enabledAttribute == null)
                {
                    CoverletDataCollectorState = CoverletDataCollectorState.Enabled;
                }
                else
                {
                    CoverletDataCollectorState = string.Equals(enabledAttribute.Value, "true", System.StringComparison.OrdinalIgnoreCase) ? CoverletDataCollectorState.Enabled : CoverletDataCollectorState.Disabled;
                }
            }
            else
            {
                return false;
            }

            XElement configurationElement = coverletDataCollectorElement.Element("Configuration");
            if (configurationElement == null)
            {
                return false;
            }
            System.Collections.Generic.List<XElement> configurationElements = configurationElement.Elements().ToList();
            if (configurationElements.Count == 0)
            {
                return false;
            }

            bool foundElements = false;
            this.GetType().GetProperties().ToList().ForEach(p =>
            {
                XElement configurationPropertyElement = configurationElements.FirstOrDefault(e => e.Name == p.Name);
                if (configurationPropertyElement != null)
                {
                    foundElements = true;
                    p.SetValue(this, configurationPropertyElement.Value);
                }
            });

            return foundElements;
        }

        public CoverletDataCollectorState CoverletDataCollectorState { get; private set; }

#pragma warning disable RCS1170 // Use read-only auto-implemented property
        public string Format { get; private set; }
        public string Exclude { get; private set; }
        public string Include { get; private set; }
        public string ExcludeByAttribute { get; private set; }
        public string ExcludeByFile { get; private set; }
        public string IncludeDirectory { get; private set; }
        public string SingleHit { get; private set; }
        public string UseSourceLink { get; private set; }
        public string IncludeTestAssembly { get; private set; }
        public string SkipAutoProps { get; private set; }
#pragma warning restore RCS1170 // Use read-only auto-implemented property
    }
}
