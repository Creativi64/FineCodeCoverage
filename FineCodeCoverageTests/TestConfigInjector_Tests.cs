using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Collection.CoverageProjectManagement;
using FineCodeCoverage.Collection.TestingPlatform.TUnit;
using FineCodeCoverage.Utilities.Wrappers;
using FineCodeCoverage.VSAbstractions.OutputWindow;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace FineCodeCoverageTests
{
    public class TestConfigInjector_Tests
    {
        private const string TestDllFile = @"C:\Project\bin\Debug\net10.0\MyTests.dll";
        private const string ExpectedTestConfigPath = @"C:\Project\bin\Debug\net10.0\MyTests.testconfig.json";
        private const string FccOutputPath = @"C:\Project\bin\Debug\net10.0\fine-code-coverage\coverage-tool-output\coverage.xml";
        private const string FccSettingsPath = @"C:\Project\bin\Debug\net10.0\fine-code-coverage\coverage-tool-output\config.xml";

        private Mock<IFileUtil> mockFileUtil;
        private Mock<ITestingPlatformVersionProvider> mockTestingPlatformVersionProvider;
        private TestConfigInjector testConfigInjector;
        private string writtenTestConfig;

        [SetUp]
        public void SetUp()
        {
            this.writtenTestConfig = null;
            this.mockFileUtil = new Mock<IFileUtil>();
            this.mockFileUtil.Setup(fileUtil => fileUtil.WriteAllText(ExpectedTestConfigPath, It.IsAny<string>()))
                .Callback<string, string>((_, contents) => this.writtenTestConfig = contents);
            this.mockTestingPlatformVersionProvider = new Mock<ITestingPlatformVersionProvider>();
            this.TestingPlatformVersion(new Version(2, 3, 0));
            this.testConfigInjector = new TestConfigInjector(
                this.mockFileUtil.Object,
                this.mockTestingPlatformVersionProvider.Object,
                new Mock<ILogger>().Object);
        }

        private void TestingPlatformVersion(Version version)
            => this.mockTestingPlatformVersionProvider
                .Setup(testingPlatformVersionProvider => testingPlatformVersionProvider.GetVersion(Path.GetDirectoryName(TestDllFile)))
                .Returns(version);

        private static ITUnitCoverageProject CreateTUnitCoverageProject()
        {
            var mockCoverageProject = new Mock<ICoverageProject>();
            mockCoverageProject.Setup(coverageProject => coverageProject.TestDllFile).Returns(TestDllFile);
            mockCoverageProject.Setup(coverageProject => coverageProject.ProjectOutputFolder).Returns(Path.GetDirectoryName(TestDllFile));
            mockCoverageProject.Setup(coverageProject => coverageProject.ProjectName).Returns("MyTests");

            var mockTUnitCoverageProject = new Mock<ITUnitCoverageProject>();
            mockTUnitCoverageProject.Setup(tUnitCoverageProject => tUnitCoverageProject.CoverageProject).Returns(mockCoverageProject.Object);
            mockTUnitCoverageProject.Setup(tUnitCoverageProject => tUnitCoverageProject.HasCoverageExtension).Returns(true);
            return mockTUnitCoverageProject.Object;
        }

        private static TUnitSettings CreateSettings(
            string userOutputPath = null,
            string userOutputFormat = null,
            bool userSuppliedSettingsPath = false,
            string settingsPath = FccSettingsPath)
            => new TUnitSettings(
                @"C:\Project\bin\Debug\net10.0\MyTests.exe",
                settingsPath,
                userSuppliedSettingsPath,
                FccOutputPath,
                userOutputPath,
                userOutputFormat,
                string.Empty);

        private void ExistingTestConfig(string json)
        {
            this.mockFileUtil.Setup(fileUtil => fileUtil.Exists(ExpectedTestConfigPath)).Returns(true);
            this.mockFileUtil.Setup(fileUtil => fileUtil.ReadAllText(ExpectedTestConfigPath)).Returns(json);
        }

        private Task<TestConfigInjectionResult> InjectAsync(TUnitSettings tUnitSettings = null)
            => this.testConfigInjector.InjectAsync(
                CreateTUnitCoverageProject(),
                tUnitSettings ?? CreateSettings(),
                CancellationToken.None);

        [Test]
        public async Task Should_Write_The_Fcc_Coverage_Options_When_There_Is_No_Existing_TestConfig_Async()
        {
            TestConfigInjectionResult result = await this.InjectAsync();

            JObject commandLineOptions = (JObject)JObject.Parse(this.writtenTestConfig)["commandLineOptions"];
            Assert.Multiple(() =>
            {
                Assert.That(result.TestConfigPath, Is.EqualTo(ExpectedTestConfigPath));
                Assert.That(commandLineOptions["coverage"].Value<bool>(), Is.True);
                Assert.That(commandLineOptions["coverage-output"].Value<string>(), Is.EqualTo(FccOutputPath));
                Assert.That(commandLineOptions["coverage-output-format"].Value<string>(), Is.EqualTo("cobertura"));
                Assert.That(commandLineOptions["coverage-settings"].Value<string>(), Is.EqualTo(FccSettingsPath));
                Assert.That(result.CoverageOutputPath, Is.EqualTo(FccOutputPath));
                Assert.That(result.CoverageOutputFormat, Is.EqualTo("cobertura"));
                Assert.That(result.CoverageSettingsPath, Is.EqualTo(FccSettingsPath));
            });
        }

        private void UseInMemoryConfig(string contents)
        {
            writtenTestConfig = contents;
            mockFileUtil.Setup(files => files.Exists(ExpectedTestConfigPath)).Returns(() => writtenTestConfig != null);
            mockFileUtil.Setup(files => files.ReadAllText(ExpectedTestConfigPath)).Returns(() => writtenTestConfig);
            mockFileUtil.Setup(files => files.DeleteFile(ExpectedTestConfigPath)).Callback(() => writtenTestConfig = null);
        }

        [Test]
        public async Task Should_Not_Replace_A_Non_Object_Options_Section_Async()
        {
            const string original = "{\"commandLineOptions\":\"invalid\"}";
            UseInMemoryConfig(original);
            Assert.That(await InjectAsync(), Is.Null);
            Assert.That(writtenTestConfig, Is.EqualTo(original));
        }

        [TestCase(null)]
        [TestCase("{\n  \"commandLineOptions\": { \"coverage\": false }, \"custom\": 42\n}")]
        public async Task Should_Restore_The_Original_Config_Exactly_Async(string original)
        {
            UseInMemoryConfig(original);
            TestConfigInjectionResult injection = await InjectAsync();
            testConfigInjector.Restore(injection);
            Assert.That(writtenTestConfig, Is.EqualTo(original));
        }

        [Test]
        public async Task Should_Preserve_Concurrent_Edits_When_Restoring_Async()
        {
            UseInMemoryConfig("{\"commandLineOptions\":{\"coverage\":false}}");
            TestConfigInjectionResult injection = await InjectAsync();
            JObject edited = JObject.Parse(writtenTestConfig);
            edited["custom"] = "added during run";
            edited["commandLineOptions"]["coverage-output"] = "new-user-path.xml";
            writtenTestConfig = edited.ToString();

            testConfigInjector.Restore(injection);

            JObject restored = JObject.Parse(writtenTestConfig);
            Assert.That(restored["custom"].Value<string>(), Is.EqualTo("added during run"));
            Assert.That(restored["commandLineOptions"]["coverage-output"].Value<string>(), Is.EqualTo("new-user-path.xml"));
            Assert.That(restored["commandLineOptions"]["coverage"].Value<bool>(), Is.False);
            Assert.That(restored["commandLineOptions"]["coverage-settings"], Is.Null);
        }

        [Test]
        public async Task Should_Not_Overwrite_A_Config_Replaced_By_A_Build_Async()
        {
            UseInMemoryConfig(null);
            TestConfigInjectionResult injection = await InjectAsync();
            writtenTestConfig = "{\"rebuilt\":true}";
            testConfigInjector.Restore(injection);
            Assert.That(writtenTestConfig, Is.EqualTo("{\"rebuilt\":true}"));
        }

        [Test]
        public void Should_Restore_Config_If_Injection_Fails_After_Writing()
        {
            UseInMemoryConfig("{}");
            var logger = new Mock<ILogger>();
            logger.Setup(log => log.LogAsync(It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new InvalidOperationException("logging failed"));
            testConfigInjector = new TestConfigInjector(mockFileUtil.Object, mockTestingPlatformVersionProvider.Object, logger.Object);

            Assert.ThrowsAsync<InvalidOperationException>(() => InjectAsync());
            Assert.That(writtenTestConfig, Is.EqualTo("{}"));
        }

        [Test]
        public async Task Should_Keep_Unrelated_Sections_Of_An_Existing_TestConfig_Async()
        {
            this.ExistingTestConfig(@"
{
  ""platformOptions"": {
    ""exitProcessOnUnhandledException"": true
  },
  ""mstest"": {
    ""execution"": { ""timeout"": ""5m"" }
  }
}");

            _ = await this.InjectAsync();

            JObject written = JObject.Parse(this.writtenTestConfig);
            Assert.Multiple(() =>
            {
                Assert.That(written["platformOptions"]["exitProcessOnUnhandledException"].Value<bool>(), Is.True);
                Assert.That(written["mstest"]["execution"]["timeout"].Value<string>(), Is.EqualTo("5m"));
                Assert.That(written["commandLineOptions"]["coverage"].Value<bool>(), Is.True);
            });
        }

        [Test]
        public async Task Should_Keep_Options_Already_Present_In_The_Existing_TestConfig_Async()
        {
            this.ExistingTestConfig(@"
{
  ""commandLineOptions"": {
    ""coverage-output"": ""C:\\theirs\\coverage.xml"",
    ""coverage-output-format"": ""xml""
  }
}");

            TestConfigInjectionResult result = await this.InjectAsync();

            JObject commandLineOptions = (JObject)JObject.Parse(this.writtenTestConfig)["commandLineOptions"];
            Assert.Multiple(() =>
            {
                Assert.That(commandLineOptions["coverage-output"].Value<string>(), Is.EqualTo(@"C:\theirs\coverage.xml"));
                Assert.That(commandLineOptions["coverage-output-format"].Value<string>(), Is.EqualTo("xml"));

                // filled in as it was not already specified
                Assert.That(commandLineOptions["coverage-settings"].Value<string>(), Is.EqualTo(FccSettingsPath));
                Assert.That(result.CoverageOutputPath, Is.EqualTo(@"C:\theirs\coverage.xml"));
                Assert.That(result.CoverageOutputFormat, Is.EqualTo("xml"));
            });
        }

        [Test]
        public async Task Should_Prefer_The_MsBuild_Property_Values_Over_The_Existing_TestConfig_Async()
        {
            this.ExistingTestConfig(@"
{
  ""commandLineOptions"": {
    ""coverage-output"": ""C:\\stale\\coverage.xml"",
    ""coverage-output-format"": ""cobertura"",
    ""coverage-settings"": ""C:\\stale\\config.xml""
  }
}");

            TestConfigInjectionResult result = await this.InjectAsync(
                CreateSettings(
                    userOutputPath: @"C:\theirs\coverage.xml",
                    userOutputFormat: "xml",
                    userSuppliedSettingsPath: true,
                    settingsPath: @"C:\theirs\config.xml"));

            Assert.Multiple(() =>
            {
                Assert.That(result.CoverageOutputPath, Is.EqualTo(@"C:\theirs\coverage.xml"));
                Assert.That(result.CoverageOutputFormat, Is.EqualTo("xml"));
                Assert.That(result.CoverageSettingsPath, Is.EqualTo(@"C:\theirs\config.xml"));
            });
        }

        [Test]
        public async Task Should_Always_Ask_For_Coverage_Async()
        {
            this.ExistingTestConfig(@"{ ""commandLineOptions"": { ""coverage"": false } }");

            _ = await this.InjectAsync();

            Assert.That(JObject.Parse(this.writtenTestConfig)["commandLineOptions"]["coverage"].Value<bool>(), Is.True);
        }

        [Test]
        public async Task Should_Not_Write_When_The_Existing_TestConfig_Cannot_Be_Read_Async()
        {
            this.ExistingTestConfig("{ this is not json");

            TestConfigInjectionResult result = await this.InjectAsync();

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Null);
                this.mockFileUtil.Verify(fileUtil => fileUtil.WriteAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            });
        }

        /*
            Reading command line options from testconfig.json arrived in Microsoft.Testing.Platform 2.3.0.
            An older platform loads the file and silently ignores the section, which would present as a
            test run that produced no coverage at all - automatic coverage must be skipped instead.
        */
        [TestCase("2.0.2")]
        [TestCase("2.2.3")]
        [TestCase("1.9.1")]
        public async Task Should_Not_Inject_When_The_Testing_Platform_Is_Too_Old_Async(string version)
        {
            this.TestingPlatformVersion(Version.Parse(version));

            TestConfigInjectionResult result = await this.InjectAsync();

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Null);
                this.mockFileUtil.Verify(fileUtil => fileUtil.WriteAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            });
        }

        [Test]
        public async Task Should_Not_Inject_When_The_Testing_Platform_Version_Is_Unknown_Async()
        {
            this.TestingPlatformVersion(null);

            Assert.That(await this.InjectAsync(), Is.Null);
        }

        [TestCase("2.3.0")]
        [TestCase("2.3.3")]
        [TestCase("3.0.0")]
        public async Task Should_Inject_When_The_Testing_Platform_Is_New_Enough_Async(string version)
        {
            this.TestingPlatformVersion(Version.Parse(version));

            Assert.That(await this.InjectAsync(), Is.Not.Null);
        }

        [Test]
        public async Task Should_Delete_Stale_Coverage_Output_Async()
        {
            this.mockFileUtil.Setup(fileUtil => fileUtil.Exists(FccOutputPath)).Returns(true);

            _ = await this.InjectAsync();

            this.mockFileUtil.Verify(fileUtil => fileUtil.DeleteFile(FccOutputPath));
        }

        [Test]
        public async Task Should_Not_Fail_When_Stale_Coverage_Output_Cannot_Be_Deleted_Async()
        {
            this.mockFileUtil.Setup(fileUtil => fileUtil.Exists(FccOutputPath)).Returns(true);
            this.mockFileUtil.Setup(fileUtil => fileUtil.DeleteFile(FccOutputPath)).Throws(new IOException("locked"));

            TestConfigInjectionResult result = await this.InjectAsync();

            Assert.That(result, Is.Not.Null);
        }
    }
}
