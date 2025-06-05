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
        private readonly IRunSettingsTemplate _runSettingsTemplate;
        private readonly IRunSettingsTemplateReplacementsFactory _runSettingsTemplateReplacementsFactory;
        private readonly IFileUtil _fileUtil;
        private XDocument _runSettingsDoc;
        private string _fccMsTestAdapterPath;

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
            IRunSettingsTemplateReplacementsFactory runSettingsTemplateReplacementsFactory)
        {
            _fileUtil = fileUtil;
            _runSettingsTemplate = runSettingsTemplate;
            _runSettingsTemplateReplacementsFactory = runSettingsTemplateReplacementsFactory;
        }

        #region analysis
        public IUserRunSettingsAnalysisResult Analyse(IEnumerable<ICoverageProject> coverageProjectsWithRunSettings, bool useMsCodeCoverage, string fccMsTestAdapterPath)
        {
            _fccMsTestAdapterPath = fccMsTestAdapterPath;
            var projectsWithFCCMsTestAdapter = new List<ICoverageProject>();
            bool specifiedMsCodeCoverage = false;
            foreach (ICoverageProject coverageProject in coverageProjectsWithRunSettings)
            {
                (bool suitable, bool projectSpecifiedMsCodeCoverage) = ValidateUserRunSettings(coverageProject.RunSettingsFile, useMsCodeCoverage);

                if (!suitable)
                {
                    return new UserRunSettingsAnalysisResult();
                }

                if (projectSpecifiedMsCodeCoverage)
                {
                    specifiedMsCodeCoverage = true;
                }

                if (ProjectHasFCCMsTestAdapter())
                {
                    projectsWithFCCMsTestAdapter.Add(coverageProject);
                }
            }

            return new UserRunSettingsAnalysisResult { Suitable = true, SpecifiedMsCodeCoverage = specifiedMsCodeCoverage, ProjectsWithFCCMsTestAdapter = projectsWithFCCMsTestAdapter };
        }

        private bool ProjectHasFCCMsTestAdapter()
        {
            XElement testAdaptersPathElement = _runSettingsDoc.GetStrictDescendant("RunSettings/RunConfiguration/TestAdaptersPaths");

            // given that add a replaceable
            if (testAdaptersPathElement == null)
            {
                return true;
            }

            string testAdaptersPaths = testAdaptersPathElement.Value;
            if (_runSettingsTemplate.HasReplaceableTestAdapter(testAdaptersPaths))
            {
                return true;
            }

            string[] paths = testAdaptersPaths.Split(';');
            return paths.Any(path => path == _fccMsTestAdapterPath);
        }

        internal (bool Suitable, bool SpecifiedMsCodeCoverage) ValidateUserRunSettings(string userRunSettingsFile, bool useMsCodeCoverage)
        {
            try
            {
                string runSettings = _fileUtil.ReadAllText(userRunSettingsFile);
                _runSettingsDoc = XDocument.Parse(runSettings);
                XElement dataCollectorsElement = _runSettingsDoc.GetStrictDescendant("RunSettings/DataCollectionRunSettings/DataCollectors");
                if (dataCollectorsElement == null)
                {
                    return (useMsCodeCoverage, false);
                }

                XElement msDataCollectorElement = RunSettingsHelper.FindMsDataCollector(dataCollectorsElement);

                return msDataCollectorElement == null
                    ? ((bool Suitable, bool SpecifiedMsCodeCoverage))(useMsCodeCoverage, false)
                    : HasCoberturaFormat(msDataCollectorElement) ?
                    ((bool Suitable, bool SpecifiedMsCodeCoverage))(true, true) :
                    ((bool Suitable, bool SpecifiedMsCodeCoverage))(useMsCodeCoverage, true);
            }
            catch
            {
                return (false, false);
            }
        }

        private static bool HasCoberturaFormat(XElement msDataCollectorElement)
        {
            XElement formatElement = msDataCollectorElement.GetStrictDescendant("Configuration/Format");
            return formatElement?.Value == "Cobertura";
        }

        #endregion

        #region AddFCCRunSettings

        public IXPathNavigable AddFCCRunSettings(
            IXPathNavigable inputRunSettingDocument,
            IRunSettingsConfigurationInfo configurationInfo,
            Dictionary<string, IUserRunSettingsProjectDetails> userRunSettingsProjectDetailsLookup,
            string fccMsTestAdapterPath) => !_runSettingsTemplate.FCCGenerated(inputRunSettingDocument)
                ? AddFCCRunSettingsActual(
                    inputRunSettingDocument, configurationInfo, userRunSettingsProjectDetailsLookup, fccMsTestAdapterPath)
                : null;

        private XPathNavigator AddFCCRunSettingsActual(
            IXPathNavigable inputRunSettingDocument,
            IRunSettingsConfigurationInfo configurationInfo,
            Dictionary<string, IUserRunSettingsProjectDetails> userRunSettingsProjectDetailsLookup,
            string fccMsTestAdapterPath)
        {
            XPathNavigator navigator = inputRunSettingDocument.CreateNavigator();
            _ = navigator.MoveToChild("RunSettings", string.Empty);
            XPathNavigator clonedNavigator = navigator.Clone();
            IRunSettingsTemplateReplacements replacements = _runSettingsTemplateReplacementsFactory.Create(
                configurationInfo.TestContainers,
                userRunSettingsProjectDetailsLookup,
                fccMsTestAdapterPath);

            EnsureTestAdaptersPathsAndReplace(navigator, replacements);
            EnsureCorrectMsDataCollectorAndReplace(clonedNavigator, replacements);
            return navigator;
        }

        private void EnsureTestAdaptersPathsAndReplace(XPathNavigator xpathNavigator, IRunSettingsTemplateReplacements replacements)
        {
            bool movedToRunConfiguration = xpathNavigator.MoveToChild("RunConfiguration", string.Empty);
            if (movedToRunConfiguration)
            {
                if (!xpathNavigator.HasChild("TestAdaptersPaths"))
                {
                    xpathNavigator.AppendChild(_runSettingsTemplate.TestAdaptersPathElement);
                }

                // todo ResultsDirectory ?
            }
            else
            {
                xpathNavigator.PrependChild(_runSettingsTemplate.RunConfigurationElement);
                _ = xpathNavigator.MoveToChild("RunConfiguration", string.Empty);
            }

            string replaced = _runSettingsTemplate.Replace(xpathNavigator.OuterXml, replacements);
            xpathNavigator.OuterXml = replaced;
        }

        private void EnsureCorrectMsDataCollectorAndReplace(XPathNavigator xpathNavigator, IRunSettingsTemplateReplacements replacements)
        {
            bool addedMsDataCollector = true;
            bool movedToDataCollectionRunSettings = xpathNavigator.MoveToChild("DataCollectionRunSettings", string.Empty);
            if (movedToDataCollectionRunSettings)
            {
                bool movedToDataCollectors = xpathNavigator.MoveToChild("DataCollectors", string.Empty);
                if (movedToDataCollectors)
                {
                    XPathNavigator msDataCollectorNavigator = MoveToMsDataCollectorFromDataCollectors(xpathNavigator);

                    if (msDataCollectorNavigator != null)
                    {
                        addedMsDataCollector = false;
                        XPathNavigator msDataCollectorNavigatorClone = msDataCollectorNavigator.Clone();
                        EnsureCorrectCoberturaFormat(msDataCollectorNavigator);
                        ReplaceExcludesIncludes(msDataCollectorNavigatorClone, replacements);
                    }
                    else
                    {
                        xpathNavigator.AppendChild(_runSettingsTemplate.MsDataCollectorElement);
                    }
                }
                else
                {
                    xpathNavigator.AppendChild(_runSettingsTemplate.DataCollectorsElement);
                }
            }
            else
            {
                xpathNavigator.AppendChild(_runSettingsTemplate.DataCollectionRunSettingsElement);
            }

            // todo - improve this
            bool disableMsDataCollector = replacements.Enabled == "false";
            if (addedMsDataCollector || disableMsDataCollector)
            {
                xpathNavigator.MoveToRoot();
                XPathNavigator dataCollectorsNavigator = xpathNavigator.SelectSingleNode("/RunSettings/DataCollectionRunSettings/DataCollectors");
                XPathNavigator msDataCollectorNavigator = MoveToMsDataCollectorFromDataCollectors(dataCollectorsNavigator);

                if (disableMsDataCollector)
                {
                    DisableMsDataCollector(msDataCollectorNavigator);
                }
                else
                {
                    ReplaceExcludesIncludes(msDataCollectorNavigator, replacements); // no need to replace if we are disabling
                }
            }
        }

        private static void DisableMsDataCollector(XPathNavigator msDataCollectorNavigator)
        {
            var element = XElement.Parse(msDataCollectorNavigator.OuterXml);
            element.SetAttributeValue("enabled", "false");
            msDataCollectorNavigator.OuterXml = element.ToString();
        }

        private static XPathNavigator MoveToMsDataCollectorFromDataCollectors(XPathNavigator navigator)
        {
            string friendlyNameXPath = $"{RunSettingsHelper.FriendlyNameAttributeName}='{RunSettingsHelper.MsDataCollectorFriendlyName}'";
            string uriXPath = $"{RunSettingsHelper.UriAttributeName}='{RunSettingsHelper.MsDataCollectorUri}'";

            return navigator.SelectSingleNode($"DataCollector[@{friendlyNameXPath} or @{uriXPath}]");
        }

        private void ReplaceExcludesIncludes(XPathNavigator msDataCollectorNavigator, IRunSettingsTemplateReplacements replacements)
        {
            string toReplace = msDataCollectorNavigator.OuterXml;
            string replaced = _runSettingsTemplate.Replace(toReplace, replacements);
            msDataCollectorNavigator.OuterXml = replaced;
        }

        private static void EnsureCorrectCoberturaFormat(XPathNavigator msDataCollectorNavigator)
        {
            bool movedToConfiguration = msDataCollectorNavigator.MoveToChild("Configuration", string.Empty);
            if (movedToConfiguration)
            {
                bool movedToFormat = msDataCollectorNavigator.MoveToChild("Format", string.Empty);
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
