using FineCodeCoverage.Engine.Model;

namespace FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage
{
    internal class TemplatedCoverageProjectRunSettingsResult : ICoverageProjectRunSettings
    {
        public ICoverageProject CoverageProject { get; set; }
        public string RunSettings { get; set; }
        public string CustomTemplatePath { get; internal set; }
        public bool ReplacedTestAdapter { get; internal set; }
    }
}