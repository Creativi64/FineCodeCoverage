namespace FineCodeCoverage.Collection.TestingPlatform.TUnit
{
    /// <summary>
    /// The coverage options that the test host will actually run with, after FCC has filled in
    /// whatever the user did not already specify.
    /// </summary>
    public sealed class TestConfigInjectionResult
    {
        public TestConfigInjectionResult(
            string testConfigPath,
            string coverageOutputPath,
            string coverageOutputFormat,
            string coverageSettingsPath)
        {
            TestConfigPath = testConfigPath;
            CoverageOutputPath = coverageOutputPath;
            CoverageOutputFormat = coverageOutputFormat;
            CoverageSettingsPath = coverageSettingsPath;
        }

        public string TestConfigPath { get; }

        /// <summary>
        /// The file FCC will look for when the test run has finished.
        /// </summary>
        public string CoverageOutputPath { get; }

        public string CoverageOutputFormat { get; }

        public string CoverageSettingsPath { get; }

        public override string ToString()
            => $"--coverage --coverage-output-format {CoverageOutputFormat} --coverage-output \"{CoverageOutputPath}\" --coverage-settings \"{CoverageSettingsPath}\"";
    }
}
