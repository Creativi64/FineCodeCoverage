using System.Collections.Generic;
using FineCodeCoverage.Engine.Model;
using Microsoft.VisualStudio.TestWindow.Extensibility;

namespace FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage
{
    internal interface IRunSettingsTemplateReplacementsFactory
    {
        IRunSettingsTemplateReplacements Create(
            IEnumerable<ITestContainer> testContainers,
            Dictionary<string, IUserRunSettingsProjectDetails> userRunSettingsProjectDetailsLookup,
            string testAdapter);

        IRunSettingsTemplateReplacements Create(ICoverageProject coverageProject, string testAdapter);
    }
}
