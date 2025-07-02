using AutoMoq;
using FineCodeCoverage.Collection.CoverageProjectManagement;
using FineCodeCoverage.Collection.CoverageProjectManagement.Settings;
using FineCodeCoverage.Options;
using FineCodeCoverage.Options.IncludesExcludes;
using FineCodeCoverage.Options.Run;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FineCodeCoverageTests
{
    public class CoverageProjectSettingsManager_Tests
    {
        private AutoMoqer autoMoqer;
        private List<XElement> settingsFileElements;
        private XElement projectSettingsElement;
        private ICoverageSettings coverageSettings;

        internal class FakeCoverageSettings
        {
            public bool Property { get; set; }
            public static List<PropertyInfo> PropertyInfos { get; } = typeof(FakeCoverageSettings).GetProperties().Cast<PropertyInfo>().ToList();
        }

        private async Task ActAsync(CoverageSettings coverageSettingsFromAppOptions,bool noXmlSettings = false)
        {
            autoMoqer = new AutoMoqer();
            var appOptions = new IncludesExcludesOptions();
            var runOptions = new RunOptions();
            object[] options = new object[] { "AnOptionObject" };
            autoMoqer.Setup<ICoverageSettingsOptionsProvider, IEnumerable<object>>(p => p.Get()).Returns(options);
            autoMoqer.Setup<ICoverageSettingsReflectionService, CoverageSettings>(coverageSettingsReflectionService => coverageSettingsReflectionService.CreateCoverageSettingsFromOptions(options))
                .Returns(coverageSettingsFromAppOptions);
            autoMoqer.Setup<ICoverageSettingsReflectionService,List<PropertyInfo>>(coverageSettingsReflectionService => coverageSettingsReflectionService.CoverageSettingsPropertyInfos)
                .Returns(FakeCoverageSettings.PropertyInfos);
            var mockCoverageProject = new Mock<ICoverageProject>();
            var coverageProjectFilePath = Path.GetTempFileName();
            mockCoverageProject.SetupGet(coverageProject => coverageProject.ProjectFilePath).Returns(coverageProjectFilePath);
            projectSettingsElement = noXmlSettings ? null : new XElement("root", "");
            autoMoqer.GetMock<ICoverageProjectSettingsProvider>().Setup(coverageProjectSettingsProvider => coverageProjectSettingsProvider.ProvideAsync(mockCoverageProject.Object))
                .ReturnsAsync(projectSettingsElement);
            settingsFileElements = noXmlSettings ? new List<XElement>() : new List<XElement> { new XElement("Settingsroot", "") };
            autoMoqer.Setup<IFCCSettingsFilesProvider, List<XElement>>(
                settingsFilesProvider => settingsFilesProvider.Provide(Path.GetDirectoryName(coverageProjectFilePath)))
                .Returns(settingsFileElements);

            var coverageProjectSettingsManager = autoMoqer.Create<CoverageProjectSettingsManager>();

            coverageSettings = await coverageProjectSettingsManager.GetSettingsAsync(mockCoverageProject.Object);
        }


        [Test]
        public async Task Should_Merge_CoverageSettings_Created_From_AppOptions_Async()
        {
            var coverageSettingsFromAppOptions = new CoverageSettings();

            await ActAsync(coverageSettingsFromAppOptions);

            Assert.That(coverageSettings, Is.SameAs(coverageSettingsFromAppOptions));
#pragma warning disable VSTHRD110 // Observe result of async calls
            autoMoqer.Verify<ISettingsMerger>(settingsMerger => settingsMerger.MergeAsync(
                coverageSettingsFromAppOptions, 
                FakeCoverageSettings.PropertyInfos,
                settingsFileElements, 
                projectSettingsElement));
#pragma warning restore VSTHRD110 // Observe result of async calls

        }

        [Test]
        public async Task Should_Not_Merge_If_No_Xml_Settings_Async()
        {
            var coverageSettingsFromAppOptions = new CoverageSettings();

            await ActAsync(coverageSettingsFromAppOptions, true);

#pragma warning disable VSTHRD110 // Observe result of async calls
            autoMoqer.Verify<ISettingsMerger>(settingsMerger => settingsMerger.MergeAsync(
                coverageSettingsFromAppOptions,
                FakeCoverageSettings.PropertyInfos,
                settingsFileElements,
                projectSettingsElement),Times.Never());
#pragma warning restore VSTHRD110 // Observe result of async calls
        }

        [Test]
        public async Task Should_Add_Common_Assembly_Excludes_Includes_Ignoring_Whitespace_Async()
        {
            var coverageSettingsFromAppOptions = new CoverageSettings
            {
                Exclude = new string[] { "oldexclude" },
                Include = new string[] { "oldinclude" },
                ModulePathsExclude = new string[] { "msexclude" },
                ModulePathsInclude = new string[] { "msinclude" },
                ExcludeAssemblies = new string[] { "excludeassembly", " " },
                IncludeAssemblies = new string[] { "includeassembly", " " }
            };

            await ActAsync(coverageSettingsFromAppOptions);

            Assert.That(coverageSettings.Exclude, Is.EquivalentTo(new string[] { "oldexclude", "[excludeassembly]*" }));
            Assert.That(coverageSettings.Include, Is.EquivalentTo(new string[] { "oldinclude", "[includeassembly]*" }));
            Assert.That(coverageSettings.ModulePathsExclude, Is.EquivalentTo(new string[] { "msexclude", ".*\\excludeassembly.dll$" }));
            Assert.That(coverageSettings.ModulePathsInclude, Is.EquivalentTo(new string[] { "msinclude", ".*\\includeassembly.dll$" }));
        }
    }
}