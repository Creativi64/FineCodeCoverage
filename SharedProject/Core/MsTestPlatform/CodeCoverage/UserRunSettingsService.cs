using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine.Model;
using Microsoft.VisualStudio.TestWindow.Extensibility;

namespace FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage
{
    [Export(typeof(IUserRunSettingsService))]
    internal class UserRunSettingsService : IUserRunSettingsService
    {
        private readonly IRunSettingsTemplate runSettingsTemplate;
        private readonly IRunSettingsTemplateReplacementsFactory runSettingsTemplateReplacementsFactory;
        private readonly IFileUtil fileUtil;
        private XDocument runSettingsDoc;
        private string fccMsTestAdapterPath;

        private class UserRunSettingsAnalysisResult : IUserRunSettingsAnalysisResult
        {
            public bool Suitable { get; set; }

            public bool SpecifiedMsCodeCoverage { get; set; }

            public List<ICoverageProject> ProjectsWithFCCMsTestAdapter { get; set; } = new List<ICoverageProject>();
        }

        [ImportingConstructor]
        public UserRunSettingsService(
            IFileUtil fileUtil,
            IRunSettingsTemplate runSettingsTemplate,
            IRunSettingsTemplateReplacementsFactory runSettingsTemplateReplacementsFactory
        )
        {
            this.fileUtil = fileUtil;
            this.runSettingsTemplate = runSettingsTemplate;
            this.runSettingsTemplateReplacementsFactory = runSettingsTemplateReplacementsFactory;
        }

        #region analysis
        public IUserRunSettingsAnalysisResult Analyse(IEnumerable<ICoverageProject> coverageProjectsWithRunSettings, bool useMsCodeCoverage, string fccMsTestAdapterPath)
        {
            this.fccMsTestAdapterPath = fccMsTestAdapterPath;
            var projectsWithFCCMsTestAdapter = new List<ICoverageProject>();
            bool specifiedMsCodeCoverage = false;
            foreach (ICoverageProject coverageProject in coverageProjectsWithRunSettings)
            {
                (bool suitable, bool projectSpecifiedMsCodeCoverage) = this.ValidateUserRunSettings(coverageProject.RunSettingsFile, useMsCodeCoverage);

                if (!suitable)
                {
                    return new UserRunSettingsAnalysisResult();
                }

                if (projectSpecifiedMsCodeCoverage)
                {
                    specifiedMsCodeCoverage = true;
                }

                if (this.ProjectHasFCCMsTestAdapter())
                {
                    projectsWithFCCMsTestAdapter.Add(coverageProject);
                }
            }

            return new UserRunSettingsAnalysisResult { Suitable = true, SpecifiedMsCodeCoverage = specifiedMsCodeCoverage, ProjectsWithFCCMsTestAdapter = projectsWithFCCMsTestAdapter };
        }

        private bool ProjectHasFCCMsTestAdapter()
        {
            XElement testAdaptersPathElement = this.runSettingsDoc.GetStrictDescendant("RunSettings/RunConfiguration/TestAdaptersPaths");
            // given that add a replaceable
            if (testAdaptersPathElement == null)
            {
                return true;
            }

            string testAdaptersPaths = testAdaptersPathElement.Value;
            if (this.runSettingsTemplate.HasReplaceableTestAdapter(testAdaptersPaths))
            {
                return true;
            }

            string[] paths = testAdaptersPaths.Split(';');
            return paths.Any(path => path == this.fccMsTestAdapterPath);
        }

        internal (bool Suitable, bool SpecifiedMsCodeCoverage) ValidateUserRunSettings(string userRunSettingsFile, bool useMsCodeCoverage)
        {
            try
            {
                string runSettings = this.fileUtil.ReadAllText(userRunSettingsFile);
                this.runSettingsDoc = XDocument.Parse(runSettings);
                XElement dataCollectorsElement = this.runSettingsDoc.GetStrictDescendant("RunSettings/DataCollectionRunSettings/DataCollectors");
                if (dataCollectorsElement == null)
                {
                    return (useMsCodeCoverage, false);
                }

                XElement msDataCollectorElement = RunSettingsHelper.FindMsDataCollector(dataCollectorsElement);

                if (msDataCollectorElement == null)
                {
                    return (useMsCodeCoverage, false);
                }

                if (HasCoberturaFormat(msDataCollectorElement))
                {
                    return (true, true);
                }

                return (useMsCodeCoverage, true);
            }
            catch
            {
                return (false, false);
            }
        }

        private static bool HasCoberturaFormat(XElement msDataCollectorElement)
        {
            XElement formatElement = msDataCollectorElement.GetStrictDescendant("Configuration/Format");
            if (formatElement == null)
            {
                return false;
            }

            return formatElement.Value == "Cobertura";
        }

        #endregion

        #region AddFCCRunSettings

        public IXPathNavigable AddFCCRunSettings(IXPathNavigable inputRunSettingDocument, IRunSettingsConfigurationInfo configurationInfo, Dictionary<string, IUserRunSettingsProjectDetails> userRunSettingsProjectDetailsLookup, string fccMsTestAdapterPath)
        {
            if (!this.runSettingsTemplate.FCCGenerated(inputRunSettingDocument))
            {
                return this.AddFCCRunSettingsActual(inputRunSettingDocument, configurationInfo, userRunSettingsProjectDetailsLookup, fccMsTestAdapterPath);
            }

            return null;
        }

        private IXPathNavigable AddFCCRunSettingsActual(IXPathNavigable inputRunSettingDocument, IRunSettingsConfigurationInfo configurationInfo, Dictionary<string, IUserRunSettingsProjectDetails> userRunSettingsProjectDetailsLookup, string fccMsTestAdapterPath)
        {
            XPathNavigator navigator = inputRunSettingDocument.CreateNavigator();
            _ = navigator.MoveToChild("RunSettings", "");
            XPathNavigator clonedNavigator = navigator.Clone();
            IRunSettingsTemplateReplacements replacements = this.runSettingsTemplateReplacementsFactory.Create(
                configurationInfo.TestContainers,
                userRunSettingsProjectDetailsLookup,
                fccMsTestAdapterPath
            );

            this.EnsureTestAdaptersPathsAndReplace(navigator, replacements);
            this.EnsureCorrectMsDataCollectorAndReplace(clonedNavigator, replacements);
            return navigator;
        }

        private void EnsureTestAdaptersPathsAndReplace(XPathNavigator xpathNavigator, IRunSettingsTemplateReplacements replacements)
        {
            bool movedToRunConfiguration = xpathNavigator.MoveToChild("RunConfiguration", "");
            if (movedToRunConfiguration)
            {
                if (!xpathNavigator.HasChild("TestAdaptersPaths"))
                {
                    xpathNavigator.AppendChild(this.runSettingsTemplate.TestAdaptersPathElement);
                }
                // todo ResultsDirectory ?

            }
            else
            {
                xpathNavigator.PrependChild(this.runSettingsTemplate.RunConfigurationElement);
                _ = xpathNavigator.MoveToChild("RunConfiguration", "");
            }

            string replaced = this.runSettingsTemplate.Replace(xpathNavigator.OuterXml, replacements);
            xpathNavigator.OuterXml = replaced;
        }

        private void EnsureCorrectMsDataCollectorAndReplace(XPathNavigator xpathNavigator, IRunSettingsTemplateReplacements replacements)
        {
            bool addedMsDataCollector = true;
            bool movedToDataCollectionRunSettings = xpathNavigator.MoveToChild("DataCollectionRunSettings", "");
            if (movedToDataCollectionRunSettings)
            {
                bool movedToDataCollectors = xpathNavigator.MoveToChild("DataCollectors", "");
                if (movedToDataCollectors)
                {
                    XPathNavigator msDataCollectorNavigator = this.MoveToMsDataCollectorFromDataCollectors(xpathNavigator);

                    if (msDataCollectorNavigator != null)
                    {
                        addedMsDataCollector = false;
                        XPathNavigator msDataCollectorNavigatorClone = msDataCollectorNavigator.Clone();
                        this.EnsureCorrectCoberturaFormat(msDataCollectorNavigator);
                        this.ReplaceExcludesIncludes(msDataCollectorNavigatorClone, replacements);
                    }
                    else
                    {
                        xpathNavigator.AppendChild(this.runSettingsTemplate.MsDataCollectorElement);
                    }
                }
                else
                {
                    xpathNavigator.AppendChild(this.runSettingsTemplate.DataCollectorsElement);
                }
            }
            else
            {
                xpathNavigator.AppendChild(this.runSettingsTemplate.DataCollectionRunSettingsElement);
            }

            // todo - improve this 
            bool disableMsDataCollector = replacements.Enabled == "false";
            if (addedMsDataCollector || disableMsDataCollector)
            {
                xpathNavigator.MoveToRoot();
                XPathNavigator dataCollectorsNavigator = xpathNavigator.SelectSingleNode("/RunSettings/DataCollectionRunSettings/DataCollectors");
                XPathNavigator msDataCollectorNavigator = this.MoveToMsDataCollectorFromDataCollectors(dataCollectorsNavigator);

                if (disableMsDataCollector)
                {
                    this.DisableMsDataCollector(msDataCollectorNavigator);
                }
                else
                {
                    this.ReplaceExcludesIncludes(msDataCollectorNavigator, replacements); // no need to replace if we are disabling
                }
            }
        }


        private void DisableMsDataCollector(XPathNavigator msDataCollectorNavigator)
        {
            var element = XElement.Parse(msDataCollectorNavigator.OuterXml);
            element.SetAttributeValue("enabled", "false");
            msDataCollectorNavigator.OuterXml = element.ToString();
        }

        private XPathNavigator MoveToMsDataCollectorFromDataCollectors(XPathNavigator navigator)
        {
            string friendlyNameXPath = $"{RunSettingsHelper.FriendlyNameAttributeName}='{RunSettingsHelper.MsDataCollectorFriendlyName}'";
            string uriXPath = $"{RunSettingsHelper.UriAttributeName}='{RunSettingsHelper.MsDataCollectorUri}'";

            return navigator.SelectSingleNode($"DataCollector[@{friendlyNameXPath} or @{uriXPath}]");
        }

        private void ReplaceExcludesIncludes(XPathNavigator msDataCollectorNavigator, IRunSettingsTemplateReplacements replacements)
        {
            string toReplace = msDataCollectorNavigator.OuterXml;
            string replaced = this.runSettingsTemplate.Replace(toReplace, replacements);
            msDataCollectorNavigator.OuterXml = replaced;
        }

        private void EnsureCorrectCoberturaFormat(XPathNavigator msDataCollectorNavigator)
        {
            bool movedToConfiguration = msDataCollectorNavigator.MoveToChild("Configuration", "");
            if (movedToConfiguration)
            {
                bool movedToFormat = msDataCollectorNavigator.MoveToChild("Format", "");
                if (movedToFormat)
                {
                    if (msDataCollectorNavigator.InnerXml != "Cobertura")
                    {
                        msDataCollectorNavigator.InnerXml = "Cobertura";
                    }
                }
                else
                {
                    msDataCollectorNavigator.AppendChild("<Format>Cobertura</Format>");
                }
            }
            else
            {
                msDataCollectorNavigator.AppendChild("<Configuration><Format>Cobertura</Format></Configuration>");
            }
        }

        #endregion
    }

}
