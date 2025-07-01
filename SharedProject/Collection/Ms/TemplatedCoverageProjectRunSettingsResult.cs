using FineCodeCoverage.Collection.CoverageProjectManagement;

namespace FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage
{
    internal sealed class TemplatedCoverageProjectRunSettingsResult : ICoverageProjectRunSettings
    {
        public ICoverageProject CoverageProject { get; set; }

        public string RunSettings { get; set; }

        public string CustomTemplatePath { get; internal set; }

        public bool ReplacedTestAdapter { get; internal set; }
    }
}
