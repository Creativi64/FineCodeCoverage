using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Options;
using NUnit.Framework;

namespace FineCodeCoverageTests
{
    internal class CoverageSettingsReflectionService_Tests
    {
        [Test]
        public void Should_Copy_From_AppOptions_To_CoverageSettings()
        {
            var coverageSettingsReflectionService = new CoverageSettingsReflectionService();
 
            var appOptions = new AppOptions
            {
                CoverletConsoleCustomPath = "CoverletConsoleCustomPath", // ICoverletOptions

                OpenCoverTarget = "OpenCoverTarget", // IOpenCoverOptions

                IncludeTestAssembly = true, // IFCCCommonIncludesExcludes

                Enabled = true, // IEnabledOption

                ModulePathsInclude = new string[] { "include1", "include2" }, // IMsCodeCoverageIncludesExcludesOptions

                Exclude = new string[] { "exclude1", "exclude2" }, // IOpenCoverCoverletExcludeIncludeOptions
            };

            var coverageSettings = coverageSettingsReflectionService.CreateCoverageSettingsFromAppOptions(appOptions);

            Assert.That(coverageSettings.CoverletConsoleCustomPath, Is.SameAs(appOptions.CoverletConsoleCustomPath));
            Assert.That(coverageSettings.OpenCoverTarget, Is.SameAs(appOptions.OpenCoverTarget));
            Assert.That(coverageSettings.IncludeTestAssembly, Is.EqualTo(appOptions.IncludeTestAssembly));
            Assert.That(coverageSettings.Enabled, Is.EqualTo(appOptions.Enabled));
            // arrays are cloned
            Assert.That(coverageSettings.ModulePathsInclude, Is.Not.SameAs(appOptions.ModulePathsInclude));
            Assert.That(coverageSettings.ModulePathsInclude, Is.EqualTo(appOptions.ModulePathsInclude));
            Assert.That(coverageSettings.Exclude, Is.EqualTo(appOptions.Exclude));
        }
    }
}
