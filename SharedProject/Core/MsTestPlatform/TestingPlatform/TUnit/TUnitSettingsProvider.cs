using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Options;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    [Export(typeof(ITUnitSettingsProvider))]
    internal class TUnitSettingsProvider : ITUnitSettingsProvider
    {
        private readonly IFileUtil _fileUtil;
        private readonly IXmlUtils _xmlUtils;
        private readonly IRunSettingsToConfiguration _runSettingsToConfiguration;
        private readonly IOptionsProvider<RunOptions> _runOptionsProvider;
        private readonly IEnvironment _environment;
        private int _fccRunWhenTestsExceed;
        private bool _fccRunWhenTestsFail;

        [ImportingConstructor]
        public TUnitSettingsProvider(
            IFileUtil fileUtil,
            IXmlUtils xmlUtils,
            IRunSettingsToConfiguration runSettingsToConfiguration,
            IOptionsProvider<RunOptions> runOptionsProvider,
            IEnvironment environment
        )
        {
            this._fileUtil = fileUtil;
            this._xmlUtils = xmlUtils;
            this._runSettingsToConfiguration = runSettingsToConfiguration;
            this._runOptionsProvider = runOptionsProvider;
            this._environment = environment;
            this.TakeFCCOptions(runOptionsProvider.Get());
            this._runOptionsProvider.OptionsChanged += this.TakeFCCOptions;
        }

        private void TakeFCCOptions(RunOptions appOptions)
        {
            this._fccRunWhenTestsExceed = appOptions.RunWhenTestsExceed;
            this._fccRunWhenTestsFail = appOptions.RunWhenTestsFail;
        }

        public async Task<TUnitSettings> ProvideAsync(ITUnitCoverageProject tUnitCoverageProject, CancellationToken cancellationToken)
        {
            _ = await tUnitCoverageProject.CoverageProject.PrepareForCoverageAsync(cancellationToken, false);
            string coberturaPath = GetCoberturaPath(tUnitCoverageProject);
            CommandLineParseResult commandLineParseResult = tUnitCoverageProject.CommandLineParseResult;
            // todo commandLineParseResult.HasError
            string configurationPathArgument = null;
            var additionalArgsStringBuilder = new StringBuilder();
            string ignoreExitCodeArg = null;
            int? minimumExpectedTests = null;
            foreach (CommandLineParseOption option in commandLineParseResult.Options)
            {
                switch (option.Name)
                {
                    case "coverage":
                    case "coverage-output-format":
                    case "coverage-output"://for now will use own
                        break;
                    case "coverage-settings":
                    case "settings":
                        string arg = option.Arguments.FirstOrDefault();
                        if (arg != null && ConfigurationPathArgExists(arg))
                        {
                            configurationPathArgument = arg;
                        }

                        break;
                    case "ignore-exit-code":
                        ignoreExitCodeArg = option.Arguments.FirstOrDefault();
                        break;
                    case "minimum-expected-tests":
                        string minExpectedTestsArg = option.Arguments.FirstOrDefault();
                        if (minExpectedTestsArg != null && int.TryParse(minExpectedTestsArg, out int result))
                        {
                            minimumExpectedTests = result;
                        }

                        break;
                    default:
                        AddToAdditionalArgs($"--{option.Name} {string.Join(" ", option.Arguments)}");
                        break;
                }
            }

            AddToAdditionalArgs(this.GetMinimumExpectedTestsPart(minimumExpectedTests));
            AddToAdditionalArgs(this.GetIgnoreExitCodePart(ignoreExitCodeArg));

            string configurationPath = await this.GetConfigurationPathAsync(tUnitCoverageProject, configurationPathArgument, cancellationToken);
            return new TUnitSettings(tUnitCoverageProject.ExePath, configurationPath, coberturaPath, additionalArgsStringBuilder.ToString());

            bool ConfigurationPathArgExists(string pathArg)
            {
                pathArg = pathArg.Replace("\"", "").Replace("'", "");
                return this._fileUtil.Exists(pathArg);
            }

            void AddToAdditionalArgs(string part)
            {
                if (string.IsNullOrEmpty(part))
                {
                    return;
                }

                _ = additionalArgsStringBuilder.Append($" {part}");
            }
        }

        private string GetMinimumExpectedTestsPart(int? minimumExpectedTestsArg)
        {
            // non zero positive integer
            if (!minimumExpectedTestsArg.HasValue && this._fccRunWhenTestsExceed > 1)
            {
                minimumExpectedTestsArg = this._fccRunWhenTestsExceed - 1;
            }

            return minimumExpectedTestsArg.HasValue ? $"--minimum-expected-tests {minimumExpectedTestsArg}" : null;
        }

        private string GetIgnoreExitCodePart(string ignoreExitCodeArg)
        {
            string ignoreExitCodeString = this.GetIgnoreExitCodeString(ignoreExitCodeArg);
            List<int> ignoredExitCodes = GetIgnoredExitCodes(ignoreExitCodeString);
            if (!ignoredExitCodes.Contains(2) && this._fccRunWhenTestsFail)
            {
                ignoredExitCodes.Add(2);
            }

            return ignoredExitCodes.Count != 0 ? $"--ignore-exit-code {string.Join(";", ignoredExitCodes)}" : null;
        }

        private string GetIgnoreExitCodeString(string ignoreExitCodesArg)
        {
            string environmentVariableValue = this._environment.GetEnvironmentVariable("TESTINGPLATFORM_EXITCODE_IGNORE");
            return environmentVariableValue ?? ignoreExitCodesArg ?? "";
        }

        private static List<int> GetIgnoredExitCodes(string exitCodes)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(exitCodes))
                {
                    return Enumerable.Empty<int>().ToList();
                }

                string[] codes = exitCodes.Split(';');
                return codes.Select(code => int.Parse(code)).ToList();
            }
            catch
            {
                return Enumerable.Empty<int>().ToList();
            }
        }

        private async Task<string> GetConfigurationPathAsync(
            ITUnitCoverageProject tUnitCoverageProject,
            string configurationPathArgument,
            CancellationToken cancellationToken
        )
        {
            if (configurationPathArgument != null)
            {
                if (tUnitCoverageProject.HasCoverageExtension)
                {
                    return configurationPathArgument;
                }

                System.Xml.Linq.XElement configurationOrRunSettingsElement = this._xmlUtils.Load(configurationPathArgument);
                string name = configurationOrRunSettingsElement.Name.LocalName;
                if (name == "Configuration") return configurationPathArgument;
                if (name == "RunSettings")
                {
                    System.Xml.Linq.XElement configurationElement = this._runSettingsToConfiguration.ConvertToConfiguration(configurationOrRunSettingsElement);
                    if (configurationElement != null)
                    {
                        return this.WriteConfiguration(tUnitCoverageProject, this._xmlUtils.Serialize(configurationElement));
                    }
                }
            }

            return await this.WriteFCCConfigurationAsync(tUnitCoverageProject, cancellationToken);
        }

        private async Task<string> WriteFCCConfigurationAsync(ITUnitCoverageProject tUnitCoverageProject, CancellationToken cancellationToken)
        {
            string configuration = await tUnitCoverageProject.GetConfigurationAsync(cancellationToken);
            return this.WriteConfiguration(tUnitCoverageProject, configuration);
        }

        private string WriteConfiguration(ITUnitCoverageProject tUnitCoverageProject, string configuration)
        {
            Engine.Model.ICoverageProject coverageProject = tUnitCoverageProject.CoverageProject;
            string configurationPath = Path.Combine(coverageProject.CoverageOutputFolder, coverageProject.Id.ToString() + "config.xml");
            this._fileUtil.WriteAllText(configurationPath, configuration);
            return configurationPath;
        }

        private static string GetCoberturaPath(ITUnitCoverageProject tUnitCoverageProject)
        {
            Engine.Model.ICoverageProject coverageProject = tUnitCoverageProject.CoverageProject;
            return Path.Combine(coverageProject.CoverageOutputFolder, coverageProject.Id.ToString() + "coverage.xml");
        }
    }
}