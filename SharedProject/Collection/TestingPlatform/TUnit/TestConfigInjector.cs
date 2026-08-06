using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Collection.CoverageProjectManagement;
using FineCodeCoverage.Utilities.Wrappers;
using FineCodeCoverage.VSAbstractions.OutputWindow;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FineCodeCoverage.Collection.TestingPlatform.TUnit
{
    [Export(typeof(ITestConfigInjector))]
    internal sealed class TestConfigInjector : ITestConfigInjector
    {
        private const string TestConfigFileSuffix = ".testconfig.json";
        private const string CommandLineOptionsSectionName = "commandLineOptions";
        private const string CoverageOptionName = "coverage";
        private const string CoverageOutputOptionName = "coverage-output";
        private const string CoverageOutputFormatOptionName = "coverage-output-format";
        private const string CoverageSettingsOptionName = "coverage-settings";

        /*
            Reading command line options from testconfig.json arrived in Microsoft.Testing.Platform 2.3.0.
            Earlier versions load the file but silently ignore a commandLineOptions section - verified
            against 2.0.2, which has neither JsonCommandLineOptionEntry nor the AggregatedConfiguration
            members that enumerate the section.
        */
        private static readonly Version s_minimumTestingPlatformVersion = new Version(2, 3, 0);

        private readonly IFileUtil _fileUtil;
        private readonly ITestingPlatformVersionProvider _testingPlatformVersionProvider;
        private readonly ILogger _logger;

        [ImportingConstructor]
        public TestConfigInjector(
            IFileUtil fileUtil,
            ITestingPlatformVersionProvider testingPlatformVersionProvider,
            ILogger logger)
        {
            _fileUtil = fileUtil;
            _testingPlatformVersionProvider = testingPlatformVersionProvider;
            _logger = logger;
        }

        public async Task<TestConfigInjectionResult> InjectAsync(
            ITUnitCoverageProject tUnitCoverageProject,
            TUnitSettings tUnitSettings,
            CancellationToken cancellationToken)
        {
            ICoverageProject coverageProject = tUnitCoverageProject.CoverageProject;
            if (!await CanInjectAsync(coverageProject))
            {
                return null;
            }

            string testConfigPath = GetTestConfigPath(coverageProject);

            JObject testConfig = await LoadAsync(testConfigPath, coverageProject.ProjectName);
            if (testConfig == null)
            {
                return null;
            }

            var commandLineOptions = testConfig[CommandLineOptionsSectionName] as JObject;
            if (commandLineOptions == null)
            {
                commandLineOptions = new JObject();
                testConfig[CommandLineOptionsSectionName] = commandLineOptions;
            }

            // FCC is enabled for this project, so coverage is always asked for.  Everything else is
            // filled in only when neither an msbuild property nor the existing testconfig supplied it.
            commandLineOptions[CoverageOptionName] = true;

            string outputPath = FillGap(commandLineOptions, CoverageOutputOptionName, tUnitSettings.UserOutputPath, tUnitSettings.FccOutputPath);
            string outputFormat = FillGap(commandLineOptions, CoverageOutputFormatOptionName, tUnitSettings.UserOutputFormat, TUnitSettings.DefaultOutputFormat);
            string settingsPath = FillGap(
                commandLineOptions,
                CoverageSettingsOptionName,
                tUnitSettings.UserSuppliedSettingsPath ? tUnitSettings.SettingsPath : null,
                tUnitSettings.SettingsPath);

            cancellationToken.ThrowIfCancellationRequested();
            DeleteStaleCoverageOutput(outputPath);

            _fileUtil.WriteAllText(testConfigPath, testConfig.ToString(Formatting.Indented));

            var result = new TestConfigInjectionResult(testConfigPath, outputPath, outputFormat, settingsPath);
            await _logger.LogAsync($"{coverageProject.ProjectName} - coverage injected into {testConfigPath}", result.ToString());
            return result;
        }

        private async Task<bool> CanInjectAsync(ICoverageProject coverageProject)
        {
            Version testingPlatformVersion = _testingPlatformVersionProvider.GetVersion(coverageProject.ProjectOutputFolder);
            if (testingPlatformVersion != null && testingPlatformVersion >= s_minimumTestingPlatformVersion)
            {
                return true;
            }

            string versionDescription = testingPlatformVersion == null
                ? "The Microsoft.Testing.Platform version could not be determined"
                : $"Microsoft.Testing.Platform {testingPlatformVersion} cannot read command line options from testconfig.json";
            await _logger.LogAsync(
                $"{coverageProject.ProjectName} - {versionDescription} ({s_minimumTestingPlatformVersion} or later is required).  Coverage will be collected by re-running the test host.",
                "Reference Microsoft.Testing.Platform 2.3.0 or later to collect coverage as part of the test run itself.");
            return false;
        }

        private static string GetTestConfigPath(ICoverageProject coverageProject)
            => Path.Combine(
                coverageProject.ProjectOutputFolder,
                Path.GetFileNameWithoutExtension(coverageProject.TestDllFile) + TestConfigFileSuffix);

        private async Task<JObject> LoadAsync(string testConfigPath, string projectName)
        {
            if (!_fileUtil.Exists(testConfigPath))
            {
                return new JObject();
            }

            try
            {
                return JObject.Parse(_fileUtil.ReadAllText(testConfigPath));
            }
            catch (Exception exception)
            {
                // never overwrite a file that could not be understood
                await _logger.LogAsync(
                    $"{projectName} - unable to read {testConfigPath}.  Not injecting coverage options.",
                    exception.Message);
                return null;
            }
        }

        /// <summary>
        /// Returns the value the test host will use, writing the FCC value only when the option has not
        /// already been specified by the user - through an msbuild property or in their own testconfig.json.
        /// </summary>
        private static string FillGap(JObject commandLineOptions, string optionName, string userValue, string fccValue)
        {
            if (userValue != null)
            {
                commandLineOptions[optionName] = userValue;
                return userValue;
            }

            string existing = GetExistingValue(commandLineOptions, optionName);
            if (existing != null)
            {
                return existing;
            }

            commandLineOptions[optionName] = fccValue;
            return fccValue;
        }

        private static string GetExistingValue(JObject commandLineOptions, string optionName)
        {
            JToken existing = commandLineOptions[optionName];
            if (existing == null || existing.Type == JTokenType.Null)
            {
                return null;
            }

            // a single argument is all these options take - an array is normalized by the platform
            if (existing.Type == JTokenType.Array)
            {
                existing = existing.First;
            }

            string value = existing?.ToString();
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        /*
            The coverage output folder is emptied when the coverage project is prepared, which covers
            FCC's own output path.  A user supplied --coverage-output can be anywhere, so delete that
            too - otherwise a failed or skipped run would present the previous run's report.
        */
        private void DeleteStaleCoverageOutput(string outputPath)
        {
            try
            {
                if (_fileUtil.Exists(outputPath))
                {
                    _fileUtil.DeleteFile(outputPath);
                }
            }
            catch (Exception exception)
            {
                _logger.LogFileAndForget($"Unable to delete stale coverage output {outputPath}. {exception.Message}");
            }
        }
    }
}
