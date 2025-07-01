using System.Collections.Generic;
using System.Xml.XPath;
using FineCodeCoverage.Collection.CoverageProjectManagement;
using Microsoft.VisualStudio.TestWindow.Extensibility;

namespace FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage
{
    internal interface IUserRunSettingsService
    {
        IUserRunSettingsAnalysisResult Analyse(IEnumerable<ICoverageProject> coverageProjectsWithRunSettings, bool useMsCodeCoverage, string fccMsTestAdapterPath);

        IXPathNavigable AddFCCRunSettings(IXPathNavigable inputRunSettingDocument, IRunSettingsConfigurationInfo configurationInfo, Dictionary<string, IUserRunSettingsProjectDetails> userRunSettingsProjectDetailsLookup, string fccMsTestAdapterPath);
    }
}
