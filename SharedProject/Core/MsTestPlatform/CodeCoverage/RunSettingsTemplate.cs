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

        private readonly ReplacementLookups replacementLookups = new ReplacementLookups();

        private readonly string template;

        public string RunConfigurationElement { get; }

        private string ResultsDirectoryElement { get; }
        public string TestAdaptersPathElement { get; }
        public string DataCollectionRunSettingsElement { get; }
        public string DataCollectorsElement { get; }
        public string MsDataCollectorElement { get; }

        private const string fccMarkerElementName = "FCCGenerated";
        private readonly string msDataCollectorConfigurationElement;
        private readonly string msDataCollectorCodeCoverageElement;

        private readonly List<(string elementName, string value)> recommendedYouDoNotChangeElementsNetCore = new List<(string elementName, string value)>
        {
            ("UseVerifiableInstrumentation", "True"),
            ("AllowLowIntegrityProcesses", "True"),
            ("CollectFromChildProcesses", "True"),
            ("CollectAspDotNet", "False")
        };

        private readonly List<(string elementName, string value)> recommendedYouDoNotChangeElementsNetFramework = new List<(string elementName, string value)>
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
            this.ResultsDirectoryElement = $"<ResultsDirectory>{this.replacementLookups.ResultsDirectory}</ResultsDirectory>";
            this.TestAdaptersPathElement = $"<TestAdaptersPaths>{this.replacementLookups.TestAdapter}</TestAdaptersPaths>";
            this.RunConfigurationElement = $@"
  <RunConfiguration>
    {this.ResultsDirectoryElement}
    {this.TestAdaptersPathElement}
    <CollectSourceInformation>False</CollectSourceInformation>
  </RunConfiguration>
";
            this.msDataCollectorCodeCoverageElement = $@"
        <CodeCoverage>
            <ModulePaths>
              <Exclude>
                {this.replacementLookups.ModulePathsExclude}
              </Exclude>
              <Include>
                {this.replacementLookups.ModulePathsInclude}
              </Include>
            </ModulePaths>
            <Functions>
              <Exclude>
                {this.replacementLookups.FunctionsExclude}
              </Exclude>
              <Include>
                {this.replacementLookups.FunctionsInclude}
              </Include>
            </Functions>
            <Attributes>
              <Exclude>
                {this.replacementLookups.AttributesExclude}
              </Exclude>
              <Include>
                {this.replacementLookups.AttributesInclude}
              </Include>
            </Attributes>
            <Sources>
              <Exclude>
                {this.replacementLookups.SourcesExclude}
              </Exclude>
              <Include>
                {this.replacementLookups.SourcesInclude}
              </Include>
            </Sources>
            <CompanyNames>
              <Exclude>
                {this.replacementLookups.CompanyNamesExclude}
              </Exclude>
              <Include>
                {this.replacementLookups.CompanyNamesInclude}
              </Include>
            </CompanyNames>
            <PublicKeyTokens>
              <Exclude>
                {this.replacementLookups.PublicKeyTokensExclude}
              </Exclude>
              <Include>
                {this.replacementLookups.PublicKeyTokensInclude}
              </Include>
            </PublicKeyTokens>
          </CodeCoverage>
";
            this.msDataCollectorConfigurationElement = $@"
        <Configuration>
          {this.msDataCollectorCodeCoverageElement}
          <Format>Cobertura</Format>
          <{fccMarkerElementName}/>
        </Configuration>
";
            this.MsDataCollectorElement = $@"
      <DataCollector friendlyName='Code Coverage' enabled='{this.replacementLookups.Enabled}'>
        {this.msDataCollectorConfigurationElement}
      </DataCollector>
";
            this.DataCollectorsElement = $@"
    <DataCollectors>
      {this.MsDataCollectorElement}
    </DataCollectors>
";
            this.DataCollectionRunSettingsElement = $@"
  <DataCollectionRunSettings>
    {this.DataCollectorsElement}
  </DataCollectionRunSettings>
";

            this.template = $@"<?xml version='1.0' encoding='utf-8'?>
<RunSettings>
{this.RunConfigurationElement}
{this.DataCollectionRunSettingsElement}
</RunSettings>
";
        }

        public string Get() => this.template;

        public ITemplateReplacementResult ReplaceTemplate(
            string runSettingsTemplate,
            IRunSettingsTemplateReplacements replacements,
            bool isNetFramework
        )
        {
            bool replacedTestAdapter = this.HasReplaceableTestAdapter(runSettingsTemplate);
            string replacedRunSettingsTemplate = this.Replace(runSettingsTemplate, replacements);

            return new TemplateReplaceResult
            {
                ReplacedTestAdapter = replacedTestAdapter,
                Replaced = this.AddRecommendedYouDoNotChangeElementsIfNotProvided(replacedRunSettingsTemplate, isNetFramework)
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
                List<(string elementName, string value)> recommendedYouDoNotChangeElementsDetails = isNetFramework ? this.recommendedYouDoNotChangeElementsNetFramework : this.recommendedYouDoNotChangeElementsNetCore;
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
                .Replace(this.replacementLookups.ResultsDirectory, SafeFilePathEscape(replacements.ResultsDirectory))
                .Replace(this.replacementLookups.TestAdapter, SafeFilePathEscape(replacements.TestAdapter))
                .Replace(this.replacementLookups.Enabled, replacements.Enabled)
                .Replace(this.replacementLookups.ModulePathsExclude, replacements.ModulePathsExclude)
                .Replace(this.replacementLookups.ModulePathsInclude, replacements.ModulePathsInclude)
                .Replace(this.replacementLookups.FunctionsExclude, replacements.FunctionsExclude)
                .Replace(this.replacementLookups.FunctionsInclude, replacements.FunctionsInclude)
                .Replace(this.replacementLookups.AttributesExclude, replacements.AttributesExclude)
                .Replace(this.replacementLookups.AttributesInclude, replacements.AttributesInclude)
                .Replace(this.replacementLookups.SourcesExclude, replacements.SourcesExclude)
                .Replace(this.replacementLookups.SourcesInclude, replacements.SourcesInclude)
                .Replace(this.replacementLookups.CompanyNamesExclude, replacements.CompanyNamesExclude)
                .Replace(this.replacementLookups.CompanyNamesInclude, replacements.CompanyNamesInclude)
                .Replace(this.replacementLookups.PublicKeyTokensExclude, replacements.PublicKeyTokensExclude)
                .Replace(this.replacementLookups.PublicKeyTokensInclude, replacements.PublicKeyTokensInclude);

        #region custom
        private void EnsureRunConfigurationEssentials(XElement runConfiguration)
        {
            AddIfNotPresent(runConfiguration, "ResultsDirectory", this.ResultsDirectoryElement, null, true);
            AddIfNotPresent(runConfiguration, "TestAdaptersPaths", this.TestAdaptersPathElement, null, false);
        }

        private void EnsureRunConfiguration(XElement runSettingsElement)
            => AddIfNotPresent(
                runSettingsElement,
                "RunConfiguration",
                this.RunConfigurationElement,
                this.EnsureRunConfigurationEssentials,
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
                this.MsDataCollectorElement,
                msDataCollector =>
                {
                    this.AddEnabledReplacementAttributeIfNotPresent(msDataCollector);
                    XElement msDataCollectorConfiguration = this.GetOrAddConfigurationElement(msDataCollector);
                    AddOrCorrectFormat(msDataCollectorConfiguration);
                    AddFCCGeneratedIfNotPresent(msDataCollectorConfiguration);
                });

        private XElement GetOrAddConfigurationElement(XElement msDataCollector)
        {
            AddIfNotPresent(msDataCollector, "Configuration", this.msDataCollectorConfigurationElement, this.AddCodeCoverageIfNotPresent, false);
            return msDataCollector.Element("Configuration");
        }

        private void AddCodeCoverageIfNotPresent(XElement configurationElement)
            => AddIfNotPresent(configurationElement, "CodeCoverage", this.msDataCollectorCodeCoverageElement, null, true);

        private void AddEnabledReplacementAttributeIfNotPresent(XElement msDataCollector)
        {
            XAttribute enabledAttribute = msDataCollector.Attribute("enabled");
            if (enabledAttribute == null)
            {
                msDataCollector.Add(new XAttribute("enabled", this.replacementLookups.Enabled));
            }
        }

        private static void AddFCCGeneratedIfNotPresent(XElement msDataCollectorConfiguration)
        {
            if (msDataCollectorConfiguration.Element(fccMarkerElementName) == null)
            {
                msDataCollectorConfiguration.Add(XElement.Parse($"<{fccMarkerElementName}/>"));
            }
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
                this.DataCollectorsElement,
                this.EnsureMsDataCollectorElement);

        private void EnsureMsDataCollector(XElement runSettingsElement)
            => AddIfNotPresent(
                runSettingsElement,
                "DataCollectionRunSettings",
                this.DataCollectionRunSettingsElement,
                this.EnsureDataCollectorsElement,
                false);

        public string ConfigureCustom(string runSettingsTemplate)
        {
            var runSettingsDocument = XDocument.Parse(runSettingsTemplate);
            XElement runSettingsElement = runSettingsDocument.Element("RunSettings");

            this.EnsureRunConfiguration(runSettingsElement);
            this.EnsureMsDataCollector(runSettingsElement);

            return runSettingsDocument.ToXmlString();
        }
        #endregion

        public bool FCCGenerated(IXPathNavigable inputRunSettingDocument)
        {
            XPathNavigator navigator = inputRunSettingDocument.CreateNavigator();
            return navigator.SelectSingleNode($"//{fccMarkerElementName}") != null;

        }

        public bool HasReplaceableTestAdapter(string replaceable) => replaceable.Contains(this.replacementLookups.TestAdapter);
    }
}
