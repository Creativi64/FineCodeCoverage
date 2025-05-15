using AutoMoq;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Options;
using FineCodeCoverage.Output;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FineCodeCoverageTests
{
    public class SettingsMerger_Tests
    {
        private AutoMoqer mocker;
        private SettingsMerger settingsMerger;
        private static readonly List<PropertyInfo> CoverageSettingsPropertyInfos = new CoverageSettingsReflectionService().CoverageSettingsPropertyInfos;
        [SetUp]
        public void SetUp()
        {
            mocker = new AutoMoqer();
            settingsMerger = mocker.Create<SettingsMerger>();
        }

        [Test]
        public async Task Should_Overwrite_GlobalOptions_Bool_Properties_From_Settings_File_Async()
        {
            var coverageSettings = new CoverageSettings();
            var settingsFileElement = CreateIncludeReferencedProjectsElement(true);
            await settingsMerger.MergeAsync(coverageSettings, CoverageSettingsPropertyInfos, new List<XElement> { settingsFileElement}, null);

            Assert.That(coverageSettings.IncludeReferencedProjects, Is.True);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task Should_Overwrite_GlobalOptions_Bool_Properties_From_Settings_File_In_Order_Async(bool last)
        {
            var coverageSettings = new CoverageSettings();
            var settingsFileElementTop = CreateIncludeReferencedProjectsElement(!last);
            var settingsFileElementLast = CreateIncludeReferencedProjectsElement(last);
            await settingsMerger.MergeAsync(
                coverageSettings,
                CoverageSettingsPropertyInfos,
                new List<XElement> { settingsFileElementTop, settingsFileElementLast },
                null);

            Assert.AreEqual(last, coverageSettings.IncludeReferencedProjects);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task Should_Overwrite_GlobalOptions_Bool_Properties_From_Project_Async(bool projectIncludeReferencedProject)
        {
            var coverageSettings = new CoverageSettings();
            var settingsFileElement = CreateIncludeReferencedProjectsElement(!projectIncludeReferencedProject);
            var projectElement = CreateIncludeReferencedProjectsElement(projectIncludeReferencedProject);
            await settingsMerger.MergeAsync(
                coverageSettings,
                CoverageSettingsPropertyInfos,
                new List<XElement> { settingsFileElement },
                projectElement);

            Assert.AreEqual(projectIncludeReferencedProject, coverageSettings.IncludeReferencedProjects);
        }

        [Test]
        public async Task Should_Overwrite_Enum_Properties_Async()
        {
            var coverageSettings = new CoverageSettings
            {
                OpenCoverRegister = OpenCoverRegister.User
            };
            var enumElement = XElement.Parse($@"
<Root>
    <OpenCoverRegister>Path64</OpenCoverRegister>
</Root>
");

            await settingsMerger.MergeAsync(
                coverageSettings,
                CoverageSettingsPropertyInfos,
                new List<XElement> { },
                enumElement);

            Assert.AreEqual(OpenCoverRegister.Path64, coverageSettings.OpenCoverRegister);
        }

        [Test]
        public async Task Should_Overwrite_String_Properties_Async()
        {
            var coverageSettings = new CoverageSettings
            {
                CoverletConsoleCustomPath = "CoverletConsoleCustomPath"
            };

            var stringElement = XElement.Parse($@"
<Root>
    <CoverletConsoleCustomPath>Overridden</CoverletConsoleCustomPath>
</Root>
");

            await settingsMerger.MergeAsync(
                coverageSettings,
                CoverageSettingsPropertyInfos,
                new List<XElement> { },
                stringElement);

            Assert.AreEqual("Overridden", coverageSettings.CoverletConsoleCustomPath);
        }

        [Test]
        public async Task Should_Overwrite_String_Array_By_Default_Async()
        {
            var coverageSettings = new CoverageSettings
            {
                Exclude = new string[] { "global" }
            };
            var stringArrayElement = XElement.Parse($@"
            <Root>
              <Exclude>
                1
                2
              </Exclude>
            </Root>
            ");

            await settingsMerger.MergeAsync(
                coverageSettings,
                CoverageSettingsPropertyInfos,
                new List<XElement> { },
                stringArrayElement);

            Assert.AreEqual(new string[] { "1", "2" }, coverageSettings.Exclude);
        }

        [Test]
        public async Task Should_Overwrite_String_Array_DefaultMerge_False_Async()
        {
            var coverageSettings = new CoverageSettings
            {
                Exclude = new string[] { "global" }
            };
            var stringArrayElement = XElement.Parse($@"
<Root defaultMerge='false'>
  <Exclude>
    1
    2
  </Exclude>
</Root>
");

            await settingsMerger.MergeAsync(
                coverageSettings,
                CoverageSettingsPropertyInfos,
                new List<XElement> { },
                stringArrayElement);

            Assert.AreEqual(new string[] { "1", "2" }, coverageSettings.Exclude);
        }

        [Test]
        public async Task Should_Overwrite_String_Array_DefaultMerge_True_Property_Merge_False_Async()
        {
            var coverageSettings = new CoverageSettings
            {
                Exclude = new string[] { "global" }
            };
            var stringArrayElement = XElement.Parse($@"
            <Root defaultMerge='true'>
              <Exclude merge='false'>
                1
                2
              </Exclude>
            </Root>
            ");

            await settingsMerger.MergeAsync(
                coverageSettings,
                CoverageSettingsPropertyInfos,
                new List<XElement> { },
                stringArrayElement);

            Assert.AreEqual(new string[] { "1", "2" }, coverageSettings.Exclude);
        }

        [Test]
        public async Task Should_Overwrite_String_Array_DefaultMerge_Not_Bool_Async()
        {
            var coverageSettings = new CoverageSettings
            {
                Exclude = new string[] { "global" }
            };
            var stringArrayElement = XElement.Parse($@"
            <Root defaultMerge='xxx'>
              <Exclude>
                1
                2
              </Exclude>
            </Root>
            ");

            await settingsMerger.MergeAsync(
                coverageSettings,
                CoverageSettingsPropertyInfos,
                new List<XElement> { },
                stringArrayElement);

            Assert.AreEqual(new string[] { "1", "2" }, coverageSettings.Exclude);
        }

        [Test]
        public async Task Should_Merge_String_Array_If_DefaultMerge_Async()
        {
            var coverageSettings = new CoverageSettings
            {
                Exclude = new string[] { "global" }
            };

            var stringArrayElement = XElement.Parse($@"
            <Root defaultMerge='true'>
              <Exclude>
                1
                2
              </Exclude>
            </Root>
            ");

            await settingsMerger.MergeAsync(
                coverageSettings,
                CoverageSettingsPropertyInfos,
                new List<XElement> { },
                stringArrayElement);

            Assert.AreEqual(new string[] { "global", "1", "2" }, coverageSettings.Exclude);
        }

        [Test]
        public async Task Should_Merge_If_Property_Element_Merge_Async()
        {
            var coverageSettings = new CoverageSettings
            {
                Exclude = new string[] { "global" }
            };

            var stringArrayElement = XElement.Parse($@"
            <Root defaultMerge='false'>
              <Exclude merge='true'>
                1
                2
              </Exclude>
            </Root>
            ");

            await settingsMerger.MergeAsync(
                coverageSettings,
                CoverageSettingsPropertyInfos,
                new List<XElement> { },
                stringArrayElement);

            Assert.AreEqual(new string[] { "global", "1", "2" }, coverageSettings.Exclude);
        }

        [Test]
        public async Task Should_Log_Failed_To_Get_Setting_From_Project_Settings_Exception_And_Not_Throw_Async()
        {
            
            var element = XElement.Parse($@"
            <Root>
              <OpenCoverRegister>
                DefaultX
              </OpenCoverRegister>
            </Root>
            ");

            var coverageSettings = new CoverageSettings();
            await settingsMerger.MergeAsync(
                coverageSettings,
                CoverageSettingsPropertyInfos,
                new List<XElement> { },
                element);

            var mockLogger = mocker.GetMock<ILogger>();
            mockLogger.Verify(logger => logger.LogAsync("Failed to get 'OpenCoverRegister' setting from project settings", It.IsAny<string>()));
            Assert.AreEqual(coverageSettings.OpenCoverRegister, OpenCoverRegister.Default);
        }

        [Test]
        public async Task Should_Log_Failed_To_Get_Setting_From_Settings_File_Exception_And_Not_Throw_Async()
        {

            var element = XElement.Parse($@"
<Root>
  <OpenCoverRegister>
    DefaultX
  </OpenCoverRegister>
</Root>
");

            var coverageSettings = new CoverageSettings();
            await settingsMerger.MergeAsync(
                coverageSettings,
                CoverageSettingsPropertyInfos,
                new List<XElement> { element },
                null);

            var mockLogger = mocker.GetMock<ILogger>();
            mockLogger.Verify(logger => logger.LogAsync("Failed to get 'OpenCoverRegister' setting from settings file", It.IsAny<string>()));
            Assert.AreEqual(coverageSettings.OpenCoverRegister, OpenCoverRegister.Default);
        }

        [Test]
        public async Task Should_Not_Throw_If_Merge_Current_Null_String_Array_Type_Async()
        {
            var coverageSettings = new CoverageSettings
            {
                Exclude = null
            };

            var stringArrayElement = XElement.Parse($@"
<Root>
  <Exclude merge='true'>
    1
    2
  </Exclude>
</Root>
");
            await settingsMerger.MergeAsync(
                coverageSettings,
                CoverageSettingsPropertyInfos,
                new List<XElement> { },
                stringArrayElement);

            Assert.AreEqual(new string[] { "1", "2" }, coverageSettings.Exclude);
        }

        [TestCaseSource(nameof(XmlConversionCases))]
        public void Should_Convert_Xml_Value_Correctly(XElement propertyElement, Type propertyType, object expectedConversion)
        {
            var settingsMerger = new SettingsMerger(new Mock<ILogger>().Object);
            var value = settingsMerger.GetValueFromXml(propertyElement, propertyType, "");
            Assert.AreEqual(expectedConversion, value);
        }

        [Test]
        public void Should_Throw_For_Unsupported_Conversion()
        {
            var settingsElement = XElement.Parse($"<Root><PropertyType/></Root>");
            var expectedMessage = $"Unexpected settings type Type for setting TheName in settings merger GetValueFromXml";
            Assert.Throws<Exception>(() => settingsMerger.GetValueFromXml(settingsElement, typeof(Type), "TheName"), expectedMessage);
        }

        static object[] XmlConversionCases()
        {
            XElement CreateElement(string elementName, string value)
            {
                return XElement.Parse($"<{elementName}>{value}</{elementName}>");
            }
            var cases = new object[]
            {
                
                
            };

            return cases;
        }


        private XElement CreateIncludeReferencedProjectsElement(bool include)
        {
            return XElement.Parse($@"
<Root>
    <IncludeReferencedProjects>{include}</IncludeReferencedProjects>
</Root>
");
        }
    }
}