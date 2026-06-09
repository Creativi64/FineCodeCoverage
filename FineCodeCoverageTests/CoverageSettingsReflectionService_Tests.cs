using FineCodeCoverage.Collection.CoverageProjectManagement.Settings;
using FineCodeCoverage.Options.IncludesExcludes;
using FineCodeCoverage.Options.Run;
using NUnit.Framework;

namespace FineCodeCoverageTests
{
    internal class CoverageSettingsReflectionService_Tests
    {
        [Test]
        public void Should_Copy_From_Options_To_CoverageSettings()
        {
            var coverageSettingsReflectionService = new CoverageSettingsReflectionService();

            var appOptions = new IncludesExcludesOptions
            {
                IncludeTestAssembly = true, // IFCCCommonIncludesExcludes

                ModulePathsInclude = new string[] { "include1", "include2" }, // IMsCodeCoverageIncludesExcludesOptions
            };

            var runOptions = new RunOptions
            {
                Enabled = true, // IEnabledOption
            };

            var coverageSettings = coverageSettingsReflectionService.CreateCoverageSettingsFromOptions(
                new object[] { appOptions, runOptions });

            Assert.That(coverageSettings.IncludeTestAssembly, Is.EqualTo(appOptions.IncludeTestAssembly));
            Assert.That(coverageSettings.Enabled, Is.EqualTo(runOptions.Enabled));
            // arrays are cloned
            Assert.That(coverageSettings.ModulePathsInclude, Is.Not.SameAs(appOptions.ModulePathsInclude));
            Assert.That(coverageSettings.ModulePathsInclude, Is.EqualTo(appOptions.ModulePathsInclude));
        }
    }
}
