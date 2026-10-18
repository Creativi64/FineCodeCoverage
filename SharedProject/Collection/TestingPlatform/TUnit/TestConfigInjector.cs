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

            string originalContents;
            JObject testConfig;
            try
            {
                originalContents = _fileUtil.Exists(testConfigPath) ? _fileUtil.ReadAllText(testConfigPath) : null;
                testConfig = originalContents == null ? new JObject() : JObject.Parse(originalContents);
            }
            catch (Exception exception)
            {
                await _logger.LogAsync($"{coverageProject.ProjectName} - unable to read {testConfigPath}. Not injecting coverage options.", exception.Message);
                return null;
            }

            var commandLineOptions = testConfig[CommandLineOptionsSectionName] as JObject;
            if (commandLineOptions == null && testConfig[CommandLineOptionsSectionName] != null)
            {
                await _logger.LogAsync($"{coverageProject.ProjectName} - commandLineOptions must be a JSON object. Not injecting coverage options.");
                return null;
            }

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

            var result = new TestConfigInjectionResult(testConfigPath, outputPath, outputFormat, settingsPath)
            {
                OriginalContents = originalContents,
                InjectedContents = testConfig.ToString(Formatting.Indented),
            };
            try
            {
                _fileUtil.WriteAllText(testConfigPath, result.InjectedContents);
                await _logger.LogAsync($"{coverageProject.ProjectName} - coverage injected into {testConfigPath}", result.ToString());
                cancellationToken.ThrowIfCancellationRequested();
            }
            catch
            {
                Restore(result);
                throw;
            }

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
                $"{coverageProject.ProjectName} - {versionDescription} ({s_minimumTestingPlatformVersion} or later is required). Automatic coverage will be skipped to preserve the Test Explorer selection. Use the manual MTP coverage command to run all enabled MTP projects.",
                "Reference Microsoft.Testing.Platform 2.3.0 or later to collect coverage as part of the test run itself.");
            return false;
        }

        private static string GetTestConfigPath(ICoverageProject coverageProject)
            => Path.Combine(
                coverageProject.ProjectOutputFolder,
                Path.GetFileNameWithoutExtension(coverageProject.TestDllFile) + TestConfigFileSuffix);

        public void Restore(TestConfigInjectionResult injection)
        {
            if (injection?.InjectedContents == null)
            {
                return;
            }

            try
            {
                if (!_fileUtil.Exists(injection.TestConfigPath))
                {
                    return;
                }

                string currentContents = _fileUtil.ReadAllText(injection.TestConfigPath);
                if (currentContents == injection.InjectedContents)
                {
                    if (injection.OriginalContents == null)
                    {
                        _fileUtil.DeleteFile(injection.TestConfigPath);
                    }
                    else
                    {
                        _fileUtil.WriteAllText(injection.TestConfigPath, injection.OriginalContents);
                    }

                    return;
                }

                // A user or build changed the file during execution. Undo only values still equal to
                // what this run injected, leaving any subsequent edits intact.
                JObject current = JObject.Parse(currentContents);
                JObject original = injection.OriginalContents == null ? new JObject() : JObject.Parse(injection.OriginalContents);
                JObject injected = JObject.Parse(injection.InjectedContents);
                var currentOptions = current[CommandLineOptionsSectionName] as JObject;
                var injectedOptions = (JObject)injected[CommandLineOptionsSectionName];
                var originalOptions = original[CommandLineOptionsSectionName] as JObject;
                if (currentOptions == null)
                {
                    return;
                }

                bool changed = false;
                foreach (string option in new[] { CoverageOptionName, CoverageOutputOptionName, CoverageOutputFormatOptionName, CoverageSettingsOptionName })
                {
                    JToken previous = originalOptions?[option];
                    if (JToken.DeepEquals(currentOptions[option], injectedOptions[option])
                        && !JToken.DeepEquals(previous, injectedOptions[option]))
                    {
                        if (previous == null)
                        {
                            _ = currentOptions.Remove(option);
                        }
                        else
                        {
                            currentOptions[option] = previous.DeepClone();
                        }

                        changed = true;
                    }
                }

                if (!changed)
                {
                    return;
                }

                if (!currentOptions.HasValues && original[CommandLineOptionsSectionName] == null)
                {
                    _ = current.Remove(CommandLineOptionsSectionName);
                }

                _fileUtil.WriteAllText(injection.TestConfigPath, current.ToString(Formatting.Indented));
            }
            catch (Exception exception)
            {
                _logger.LogFileAndForget($"Unable to restore {injection.TestConfigPath}. {exception.Message}");
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
