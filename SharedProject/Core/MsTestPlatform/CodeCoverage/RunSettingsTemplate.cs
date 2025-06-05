using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using FineCodeCoverage.Core.Utilities;

namespace FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage
{
    [Export(typeof(IRunSettingsTemplate))]
    internal class RunSettingsTemplate : IRunSettingsTemplate
    {
        private class ReplacementLookups : IRunSettingsTemplateReplacements
        {
            public string Enabled { get; } = "%fcc_enabled%";

            public string ResultsDirectory { get; } = "%fcc_resultsdirectory%";

            public string TestAdapter { get; } = "%fcc_testadapter%";

            public string ModulePathsExclude { get; } = "%fcc_modulepaths_exclude%";

            public string ModulePathsInclude { get; } = "%fcc_modulepaths_include%";

            public string FunctionsExclude { get; } = "%fcc_functions_exclude%";

            public string FunctionsInclude { get; } = "%fcc_functions_include%";

            public string AttributesExclude { get; } = "%fcc_attributes_exclude%";

            public string AttributesInclude { get; } = "%fcc_attributes_include%";

            public string SourcesExclude { get; } = "%fcc_sources_exclude%";

            public string SourcesInclude { get; } = "%fcc_sources_include%";

            public string CompanyNamesExclude { get; } = "%fcc_companynames_exclude%";

            public string CompanyNamesInclude { get; } = "%fcc_companynames_include%";

            public string PublicKeyTokensExclude { get; } = "%fcc_publickeytokens_exclude%";

            public string PublicKeyTokensInclude { get; } = "%fcc_publickeytokens_include%";
        }

        private readonly ReplacementLookups _replacementLookups = new ReplacementLookups();

        private readonly string _template;

        public string RunConfigurationElement { get; }

        private string ResultsDirectoryElement { get; }

        public string TestAdaptersPathElement { get; }

        public string DataCollectionRunSettingsElement { get; }

        public string DataCollectorsElement { get; }

        public string MsDataCollectorElement { get; }

        private const string FCCGeneratedMarkerElementName = "FCCGenerated";
        private readonly string _msDataCollectorConfigurationElement;
        private readonly string _msDataCollectorCodeCoverageElement;

        private readonly List<(string elementName, string value)> _recommendedYouDoNotChangeElementsNetCore = new List<(string elementName, string value)>
        {
            ("UseVerifiableInstrumentation", "True"),
            ("AllowLowIntegrityProcesses", "True"),
            ("CollectFromChildProcesses", "True"),
            ("CollectAspDotNet", "False")
        };

        private readonly List<(string elementName, string value)> _recommendedYouDoNotChangeElementsNetFramework = new List<(string elementName, string value)>
        {
            ("UseVerifiableInstrumentation", "False"),
            ("AllowLowIntegrityProcesses", "True"),
            ("CollectFromChildProcesses", "True"),
            ("CollectAspDotNet", "False")
        };

        private class TemplateReplaceResult : ITemplateReplacementResult
        {
            public string Replaced { get; set; }

            public bool ReplacedTestAdapter { get; set; }
        }

        public RunSettingsTemplate()
        {
            ResultsDirectoryElement = $"<ResultsDirectory>{_replacementLookups.ResultsDirectory}</ResultsDirectory>";
            TestAdaptersPathElement = $"<TestAdaptersPaths>{_replacementLookups.TestAdapter}</TestAdaptersPaths>";
            RunConfigurationElement = $@"
  <RunConfiguration>
    {ResultsDirectoryElement}
    {TestAdaptersPathElement}
    <CollectSourceInformation>False</CollectSourceInformation>
  </RunConfiguration>
";
            _msDataCollectorCodeCoverageElement = $@"
        <CodeCoverage>
            <ModulePaths>
              <Exclude>
                {_replacementLookups.ModulePathsExclude}
              </Exclude>
              <Include>
                {_replacementLookups.ModulePathsInclude}
              </Include>
            </ModulePaths>
            <Functions>
              <Exclude>
                {_replacementLookups.FunctionsExclude}
              </Exclude>
              <Include>
                {_replacementLookups.FunctionsInclude}
              </Include>
            </Functions>
            <Attributes>
              <Exclude>
                {_replacementLookups.AttributesExclude}
              </Exclude>
              <Include>
                {_replacementLookups.AttributesInclude}
              </Include>
            </Attributes>
            <Sources>
              <Exclude>
                {_replacementLookups.SourcesExclude}
              </Exclude>
              <Include>
                {_replacementLookups.SourcesInclude}
              </Include>
            </Sources>
            <CompanyNames>
              <Exclude>
                {_replacementLookups.CompanyNamesExclude}
              </Exclude>
              <Include>
                {_replacementLookups.CompanyNamesInclude}
              </Include>
            </CompanyNames>
            <PublicKeyTokens>
              <Exclude>
                {_replacementLookups.PublicKeyTokensExclude}
              </Exclude>
              <Include>
                {_replacementLookups.PublicKeyTokensInclude}
              </Include>
            </PublicKeyTokens>
          </CodeCoverage>
";
            _msDataCollectorConfigurationElement = $@"
        <Configuration>
          {_msDataCollectorCodeCoverageElement}
          <Format>Cobertura</Format>
          <{FCCGeneratedMarkerElementName}/>
        </Configuration>
";
            MsDataCollectorElement = $@"
      <DataCollector friendlyName='Code Coverage' enabled='{_replacementLookups.Enabled}'>
        {_msDataCollectorConfigurationElement}
      </DataCollector>
";
            DataCollectorsElement = $@"
    <DataCollectors>
      {MsDataCollectorElement}
    </DataCollectors>
";
            DataCollectionRunSettingsElement = $@"
  <DataCollectionRunSettings>
    {DataCollectorsElement}
  </DataCollectionRunSettings>
";

            _template = $@"<?xml version='1.0' encoding='utf-8'?>
<RunSettings>
{RunConfigurationElement}
{DataCollectionRunSettingsElement}
</RunSettings>
";
        }

        public string Get() => _template;

        public ITemplateReplacementResult ReplaceTemplate(
            string runSettingsTemplate,
            IRunSettingsTemplateReplacements replacements,
            bool isNetFramework
        )
        {
            bool replacedTestAdapter = HasReplaceableTestAdapter(runSettingsTemplate);
            string replacedRunSettingsTemplate = Replace(runSettingsTemplate, replacements);

            return new TemplateReplaceResult
            {
                ReplacedTestAdapter = replacedTestAdapter,
                Replaced = AddRecommendedYouDoNotChangeElementsIfNotProvided(replacedRunSettingsTemplate, isNetFramework)
            };
        }

        private string AddRecommendedYouDoNotChangeElementsIfNotProvided(string replacedRunSettingsTemplate, bool isNetFramework)
        {
            XDocument templateDocument;
            try
            {
                templateDocument = XDocument.Parse(replacedRunSettingsTemplate);
            }
            catch (XmlException exc)
            {
                throw new MsTemplateReplacementException(exc, replacedRunSettingsTemplate);
            }

            XElement msDataCollectorCodeCoverageElement = GetMsDataCollectorCodeCoverageElement(templateDocument);
            if (msDataCollectorCodeCoverageElement != null)
            {
                List<(string elementName, string value)> recommendedYouDoNotChangeElementsDetails = isNetFramework ? _recommendedYouDoNotChangeElementsNetFramework : _recommendedYouDoNotChangeElementsNetCore;
                foreach ((string elementName, string value) recommendedYouDoNotChangeElementDetails in recommendedYouDoNotChangeElementsDetails)
                {
                    string elementName = recommendedYouDoNotChangeElementDetails.elementName;
                    string value = recommendedYouDoNotChangeElementDetails.value;
                    XElement recommendedYouDoNotChangeElement = msDataCollectorCodeCoverageElement.Element(elementName);
                    if (recommendedYouDoNotChangeElement == null)
                    {
                        msDataCollectorCodeCoverageElement.Add(XElement.Parse($"<{elementName}>{value}</{elementName}>"));
                    }
                }
            }

            return templateDocument.ToXmlString();
        }

        private static XElement GetMsDataCollectorCodeCoverageElement(XDocument templateDocument)
        {
            XElement dataCollectors = templateDocument.GetStrictDescendant("RunSettings/DataCollectionRunSettings/DataCollectors");
            XElement msDataCollector = RunSettingsHelper.FindMsDataCollector(dataCollectors);
            return msDataCollector.GetStrictDescendant("Configuration/CodeCoverage");
        }

        // hacky.  due to tests
        private static string SafeFilePathEscape(string path) => path == null ? null : XmlFileEscaper.Escape(path);

        public string Replace(string templatedXml, IRunSettingsTemplateReplacements replacements)
            => templatedXml
                .Replace(_replacementLookups.ResultsDirectory, SafeFilePathEscape(replacements.ResultsDirectory))
                .Replace(_replacementLookups.TestAdapter, SafeFilePathEscape(replacements.TestAdapter))
                .Replace(_replacementLookups.Enabled, replacements.Enabled)
                .Replace(_replacementLookups.ModulePathsExclude, replacements.ModulePathsExclude)
                .Replace(_replacementLookups.ModulePathsInclude, replacements.ModulePathsInclude)
                .Replace(_replacementLookups.FunctionsExclude, replacements.FunctionsExclude)
                .Replace(_replacementLookups.FunctionsInclude, replacements.FunctionsInclude)
                .Replace(_replacementLookups.AttributesExclude, replacements.AttributesExclude)
                .Replace(_replacementLookups.AttributesInclude, replacements.AttributesInclude)
                .Replace(_replacementLookups.SourcesExclude, replacements.SourcesExclude)
                .Replace(_replacementLookups.SourcesInclude, replacements.SourcesInclude)
                .Replace(_replacementLookups.CompanyNamesExclude, replacements.CompanyNamesExclude)
                .Replace(_replacementLookups.CompanyNamesInclude, replacements.CompanyNamesInclude)
                .Replace(_replacementLookups.PublicKeyTokensExclude, replacements.PublicKeyTokensExclude)
                .Replace(_replacementLookups.PublicKeyTokensInclude, replacements.PublicKeyTokensInclude);

        #region custom
        private void EnsureRunConfigurationEssentials(XElement runConfiguration)
        {
            AddIfNotPresent(runConfiguration, "ResultsDirectory", ResultsDirectoryElement, null, true);
            AddIfNotPresent(runConfiguration, "TestAdaptersPaths", TestAdaptersPathElement, null, false);
        }

        private void EnsureRunConfiguration(XElement runSettingsElement)
            => AddIfNotPresent(
                runSettingsElement,
                "RunConfiguration",
                RunConfigurationElement,
                EnsureRunConfigurationEssentials,
                true);

        private static void AddIfNotPresent(
            XElement parent,
            string elementName,
            string elementAsString,
            Action<XElement> presentPath = null,
            bool addFirst = true) => AddIfNotPresent(parent, p => p.Element(elementName), elementAsString, presentPath, addFirst);

        private static void AddIfNotPresent(XElement parent, Func<XElement, XElement> find, string elementAsString, Action<XElement> presentPath = null, bool addFirst = true)
        {
            XElement child = find(parent);
            if (child == null)
            {
                if (addFirst)
                {
                    parent.AddFirst(XElement.Parse(elementAsString));
                }
                else
                {
                    parent.Add(XElement.Parse(elementAsString));
                }
            }
            else
            {
                presentPath?.Invoke(child);
            }
        }

        private void EnsureMsDataCollectorElement(XElement dataCollectors)
            => AddIfNotPresent(
                dataCollectors,
                _ => RunSettingsHelper.FindMsDataCollector(dataCollectors),
                MsDataCollectorElement,
                msDataCollector =>
                {
                    AddEnabledReplacementAttributeIfNotPresent(msDataCollector);
                    XElement msDataCollectorConfiguration = GetOrAddConfigurationElement(msDataCollector);
                    AddOrCorrectFormat(msDataCollectorConfiguration);
                    AddFCCGeneratedIfNotPresent(msDataCollectorConfiguration);
                });

        private XElement GetOrAddConfigurationElement(XElement msDataCollector)
        {
            AddIfNotPresent(msDataCollector, "Configuration", _msDataCollectorConfigurationElement, AddCodeCoverageIfNotPresent, false);
            return msDataCollector.Element("Configuration");
        }

        private void AddCodeCoverageIfNotPresent(XElement configurationElement)
            => AddIfNotPresent(configurationElement, "CodeCoverage", _msDataCollectorCodeCoverageElement, null, true);

        private void AddEnabledReplacementAttributeIfNotPresent(XElement msDataCollector)
        {
            XAttribute enabledAttribute = msDataCollector.Attribute("enabled");
            if (enabledAttribute != null)
            {
                return;
            }

            msDataCollector.Add(new XAttribute("enabled", _replacementLookups.Enabled));
        }

        private static void AddFCCGeneratedIfNotPresent(XElement msDataCollectorConfiguration)
        {
            if (msDataCollectorConfiguration.Element(FCCGeneratedMarkerElementName) != null)
            {
                return;
            }

            msDataCollectorConfiguration.Add(XElement.Parse($"<{FCCGeneratedMarkerElementName}/>"));
        }

        private static void AddOrCorrectFormat(XElement configuration)
        {
            XElement formatElement = configuration.Element("Format");
            if (formatElement == null)
            {
                configuration.Add(new XElement("Format", "Cobertura"));
            }
            else
            {
                formatElement.Value = "Cobertura";
            }
        }

        private void EnsureDataCollectorsElement(XElement dataCollectionRunSettings)
            => AddIfNotPresent(
                dataCollectionRunSettings,
                "DataCollectors",
                DataCollectorsElement,
                EnsureMsDataCollectorElement);

        private void EnsureMsDataCollector(XElement runSettingsElement)
            => AddIfNotPresent(
                runSettingsElement,
                "DataCollectionRunSettings",
                DataCollectionRunSettingsElement,
                EnsureDataCollectorsElement,
                false);

        public string ConfigureCustom(string runSettingsTemplate)
        {
            var runSettingsDocument = XDocument.Parse(runSettingsTemplate);
            XElement runSettingsElement = runSettingsDocument.Element("RunSettings");

            EnsureRunConfiguration(runSettingsElement);
            EnsureMsDataCollector(runSettingsElement);

            return runSettingsDocument.ToXmlString();
        }
        #endregion

        public bool FCCGenerated(IXPathNavigable inputRunSettingDocument)
        {
            XPathNavigator navigator = inputRunSettingDocument.CreateNavigator();
            return navigator.SelectSingleNode($"//{FCCGeneratedMarkerElementName}") != null;

        }

        public bool HasReplaceableTestAdapter(string replaceable) => replaceable.Contains(_replacementLookups.TestAdapter);
    }
}
