using AutoMoq;
using FineCodeCoverage.Collection.CoverageProjectManagement.Settings;
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
        public async Task Should_Overwrite_String_Array_By_Default_Async()
        {
            var coverageSettings = new CoverageSettings
            {
                ModulePathsExclude = new string[] { "global" }
            };
            var stringArrayElement = XElement.Parse($@"
            <Root>
              <ModulePathsExclude>
                1
                2
              </ModulePathsExclude>
            </Root>
            ");

            await settingsMerger.MergeAsync(
                coverageSettings,
                CoverageSettingsPropertyInfos,
                new List<XElement> { },
                stringArrayElement);

            Assert.AreEqual(new string[] { "1", "2" }, coverageSettings.ModulePathsExclude);
        }

        [Test]
        public async Task Should_Overwrite_String_Array_DefaultMerge_False_Async()
        {
            var coverageSettings = new CoverageSettings
            {
                ModulePathsExclude = new string[] { "global" }
            };
            var stringArrayElement = XElement.Parse($@"
<Root defaultMerge='false'>
  <ModulePathsExclude>
    1
    2
  </ModulePathsExclude>
</Root>
");

            await settingsMerger.MergeAsync(
                coverageSettings,
                CoverageSettingsPropertyInfos,
                new List<XElement> { },
                stringArrayElement);

            Assert.AreEqual(new string[] { "1", "2" }, coverageSettings.ModulePathsExclude);
        }

        [Test]
        public async Task Should_Overwrite_String_Array_DefaultMerge_True_Property_Merge_False_Async()
        {
            var coverageSettings = new CoverageSettings
            {
                ModulePathsExclude = new string[] { "global" }
            };
            var stringArrayElement = XElement.Parse($@"
            <Root defaultMerge='true'>
              <ModulePathsExclude merge='false'>
                1
                2
              </ModulePathsExclude>
            </Root>
            ");

            await settingsMerger.MergeAsync(
                coverageSettings,
                CoverageSettingsPropertyInfos,
                new List<XElement> { },
                stringArrayElement);

            Assert.AreEqual(new string[] { "1", "2" }, coverageSettings.ModulePathsExclude);
        }

        [Test]
        public async Task Should_Overwrite_String_Array_DefaultMerge_Not_Bool_Async()
        {
            var coverageSettings = new CoverageSettings
            {
                ModulePathsExclude = new string[] { "global" }
            };
            var stringArrayElement = XElement.Parse($@"
            <Root defaultMerge='xxx'>
              <ModulePathsExclude>
                1
                2
              </ModulePathsExclude>
            </Root>
            ");

            await settingsMerger.MergeAsync(
                coverageSettings,
                CoverageSettingsPropertyInfos,
                new List<XElement> { },
                stringArrayElement);

            Assert.AreEqual(new string[] { "1", "2" }, coverageSettings.ModulePathsExclude);
        }

        [Test]
        public async Task Should_Merge_String_Array_If_DefaultMerge_Async()
        {
            var coverageSettings = new CoverageSettings
            {
                ModulePathsExclude = new string[] { "global" }
            };

            var stringArrayElement = XElement.Parse($@"
            <Root defaultMerge='true'>
              <ModulePathsExclude>
                1
                2
              </ModulePathsExclude>
            </Root>
            ");

            await settingsMerger.MergeAsync(
                coverageSettings,
                CoverageSettingsPropertyInfos,
                new List<XElement> { },
                stringArrayElement);

            Assert.AreEqual(new string[] { "global", "1", "2" }, coverageSettings.ModulePathsExclude);
        }

        [Test]
        public async Task Should_Merge_If_Property_Element_Merge_Async()
        {
            var coverageSettings = new CoverageSettings
            {
                ModulePathsExclude = new string[] { "global" }
            };

            var stringArrayElement = XElement.Parse($@"
            <Root defaultMerge='false'>
              <ModulePathsExclude merge='true'>
                1
                2
              </ModulePathsExclude>
            </Root>
            ");

            await settingsMerger.MergeAsync(
                coverageSettings,
                CoverageSettingsPropertyInfos,
                new List<XElement> { },
                stringArrayElement);

            Assert.AreEqual(new string[] { "global", "1", "2" }, coverageSettings.ModulePathsExclude);
        }

        [Test]
        public async Task Should_Not_Throw_If_Merge_Current_Null_String_Array_Type_Async()
        {
            var coverageSettings = new CoverageSettings
            {
                ModulePathsExclude = null
            };

            var stringArrayElement = XElement.Parse($@"
<Root>
  <ModulePathsExclude merge='true'>
    1
    2
  </ModulePathsExclude>
</Root>
");
            await settingsMerger.MergeAsync(
                coverageSettings,
                CoverageSettingsPropertyInfos,
                new List<XElement> { },
                stringArrayElement);

            Assert.AreEqual(new string[] { "1", "2" }, coverageSettings.ModulePathsExclude);
        }

        [Test]
        public async Task Should_Allow_Setting_All_CoverageSettings_Properties_From_XML_Async()
        {
            var coverageSettings = new CoverageSettings() { };

            var element = XElement.Parse($@"
<root>
    {CreateElementString(nameof(CoverageSettings.AttributesExclude),new string[] { " AttributesExclude1 ", " AttributesExclude2 " })}
    {CreateElementString(nameof(CoverageSettings.Enabled), true)}
    {CreateElementString(nameof(CoverageSettings.IncludeTestAssembly), true)}
</root>
");
            await settingsMerger.MergeAsync(
                coverageSettings,
                CoverageSettingsPropertyInfos,
                new List<XElement> { },
                element);

            Assert.That(coverageSettings.AttributesExclude, Is.EqualTo(new string[] { "AttributesExclude1", "AttributesExclude2" }));
            Assert.That(coverageSettings.Enabled, Is.True);
            Assert.That(coverageSettings.IncludeTestAssembly, Is.True);
        }


        [TestCaseSource(nameof(XmlConversionCases))]
        public void Should_Convert_Xml_Value_Correctly(XElement propertyElement, Type propertyType, object expectedConversion)
        {
            var value = SettingsMerger.GetValueFromXml(propertyElement, propertyType, "");
            Assert.AreEqual(expectedConversion, value);
        }

        [Test]
        public void Should_Throw_For_Unsupported_Conversion()
        {
            var settingsElement = XElement.Parse($"<Root><PropertyType/></Root>");
            var expectedMessage = $"Unexpected settings type Type for setting TheName in settings merger GetValueFromXml";
            Assert.Throws<UnexpectedSettingsTypeException>(() => SettingsMerger.GetValueFromXml(settingsElement, typeof(Type), "TheName"), expectedMessage);
        }

        class DemoType
        {
            public bool?[] NullableBoolArray { get; set; }
            public bool[] BoolArray { get; set; }
            public bool Bool { get; set; }
            public bool? NullableBool { get; set; }
        }

        [Test]
        public void Should_Be_Able_To_SetValue_From_GetValueFromXml()
        {
            var boolArrayElement = XElement.Parse(@"
<El>
true
false
</El>
");
            var demoType = new DemoType();
            var nullableBoolArrayProperty = typeof(DemoType).GetProperty(nameof(DemoType.NullableBoolArray));
            var boolArrayProperty = typeof(DemoType).GetProperty(nameof(DemoType.BoolArray));
            SetValue(nullableBoolArrayProperty, boolArrayElement);
            Assert.That(demoType.NullableBoolArray, Is.EqualTo(new bool?[] { true, false }));
            SetValue(boolArrayProperty, boolArrayElement);
            Assert.That(demoType.BoolArray, Is.EqualTo(new bool[] { true, false }));

            var boolElement = XElement.Parse("<El>true</El>");
            var nullableBoolProperty = typeof(DemoType).GetProperty(nameof(DemoType.NullableBool));
            SetValue(nullableBoolProperty, boolElement);
            Assert.That(demoType.NullableBool.Value, Is.True);

            void SetValue(PropertyInfo property, XElement element)
            {

                var value = SettingsMerger.GetValueFromXml(element, property.PropertyType, "");

                property.SetValue(demoType, value);
            }
        }

        // add to if add additional types.
        static object[] XmlConversionCases()
        {
            var cases = new object[]
            {

                new object[]
                {
                    new XElement("Element","1.1"),
                    typeof(float),
                    1.1f
                }
            };

            return cases;
        }

        private string CreateElementString(string elementName, object value)
        {
            return $"<{elementName}>{value}</{elementName}>";
        }

        private string CreateElementString(string elementName, string[] array)
        {
            var value = string.Join(Environment.NewLine, array);
            return $"<{elementName}>{value}</{elementName}>";
        }


        private XElement CreateIncludeReferencedProjectsElement(bool include)
        {
            return XElement.Parse($@"
<Root>
    {CreateElementString(nameof(CoverageSettings.IncludeReferencedProjects),include)}
</Root>
");
        }
    }
}
