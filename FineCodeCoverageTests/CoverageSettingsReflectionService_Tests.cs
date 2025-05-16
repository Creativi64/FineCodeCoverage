using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Options;
using NUnit.Framework;

namespace FineCodeCoverageTests
{
    internal class CoverageSettingsReflectionService_Tests
    {
        [Test]
        public void Should_Copy_From_Options_To_CoverageSettings()
        {
            var coverageSettingsReflectionService = new CoverageSettingsReflectionService();
 
            var appOptions = new AppOptions
            {

                OpenCoverTarget = "OpenCoverTarget", // IOpenCoverOptions

                IncludeTestAssembly = true, // IFCCCommonIncludesExcludes

                ModulePathsInclude = new string[] { "include1", "include2" }, // IMsCodeCoverageIncludesExcludesOptions

                Exclude = new string[] { "exclude1", "exclude2" }, // IOpenCoverCoverletExcludeIncludeOptions
            };

            var runOptions = new RunOptions
            {
                Enabled = true, // IEnabledOption
            };

            var coverletOptions = new CoverletOptions
            {
                CoverletConsoleGlobal = true
            };

            var coverageSettings = coverageSettingsReflectionService.CreateCoverageSettingsFromOptions(new object[] { appOptions, runOptions, coverletOptions });

            Assert.That(coverageSettings.OpenCoverTarget, Is.SameAs(appOptions.OpenCoverTarget));
            Assert.That(coverageSettings.IncludeTestAssembly, Is.EqualTo(appOptions.IncludeTestAssembly));
            Assert.That(coverageSettings.Enabled, Is.EqualTo(runOptions.Enabled));
            // arrays are cloned
            Assert.That(coverageSettings.ModulePathsInclude, Is.Not.SameAs(appOptions.ModulePathsInclude));
            Assert.That(coverageSettings.ModulePathsInclude, Is.EqualTo(appOptions.ModulePathsInclude));
            Assert.That(coverageSettings.Exclude, Is.EqualTo(appOptions.Exclude));
            Assert.That(coverageSettings.CoverletConsoleGlobal, Is.EqualTo(coverletOptions.CoverletConsoleGlobal));
        }
    }
}
