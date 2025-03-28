using FineCodeCoverage.Core.Utilities;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.ComponentModel.Composition;
using System.IO;
using FineCodeCoverage.Options;
using System.Collections.Generic;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform.TUnit
{
    [Export(typeof(ITUnitSettingsProvider))]
    internal class TUnitSettingsProvider : ITUnitSettingsProvider
    {
        private readonly IFileUtil fileUtil;
        private readonly IXmlUtils xmlUtils;
        private readonly IRunSettingsToConfiguration runSettingsToConfiguration;
        private readonly IAppOptionsProvider appOptionsProvider;
        private int fccRunWhenTestsExceed;
        private bool fccRunWhenTestsFail;

        [ImportingConstructor]
        public TUnitSettingsProvider(
            IFileUtil fileUtil,
            IXmlUtils xmlUtils,
            IRunSettingsToConfiguration runSettingsToConfiguration,
            IAppOptionsProvider appOptionsProvider
        )
        {
            this.fileUtil = fileUtil;
            this.xmlUtils = xmlUtils;
            this.runSettingsToConfiguration = runSettingsToConfiguration;
            this.appOptionsProvider = appOptionsProvider;
            TakeFCCOptions(appOptionsProvider.Get());
            this.appOptionsProvider.OptionsChanged += TakeFCCOptions;
        }

        private void TakeFCCOptions(IAppOptions appOptions)
        {
            this.fccRunWhenTestsExceed = appOptions.RunWhenTestsExceed;
            this.fccRunWhenTestsFail = appOptions.RunWhenTestsFail;
        }

        public async Task<TUnitSettings> ProvideAsync(ITUnitCoverageProject tUnitCoverageProject, CancellationToken cancellationToken)
        {
            await tUnitCoverageProject.CoverageProject.PrepareForCoverageAsync(cancellationToken, false);
            var coberturaPath = GetCoberturaPath(tUnitCoverageProject);
            var commandLineParseResult = tUnitCoverageProject.CommandLineParseResult;
            // todo commandLineParseResult.HasError
            string configurationPathArgument = null;
            var additionalArgs = "";
            string ignoreExitCodeArg = null;
            var minimumExpectedTests = fccRunWhenTestsExceed - 1;
            foreach (var option in commandLineParseResult.Options)
            {
                switch (option.Name)
                {
                    case "coverage":
                    case "coverage-output-format":
                    case "coverage-output"://for now will use own
                        break;
                    case "coverage-settings":
                    case "settings":
                        var arg = option.Arguments.FirstOrDefault();
                        if (arg != null)
                        {
                            arg = arg.Replace("\"", "").Replace("'", "");
                            if (fileUtil.Exists(arg))
                            {
                                configurationPathArgument = arg;
                            }
                        }
                        break;
                    case "ignore-exit-code":
                        ignoreExitCodeArg = option.Arguments.FirstOrDefault();
                        break;
                    case "minimum-expected-tests"
                        var minExpectedTestsArg = option.Arguments.FirstOrDefault();
                        if (minExpectedTestsArg != null)
                        {
                            if(int.TryParse(minExpectedTestsArg, out var result))
                            {
                                minimumExpectedTests = result;
                            }
                        }
                        break;
                    default:
                        additionalArgs += $" --{option.Name} {string.Join(" ", option.Arguments)}";
                        break;
                }
            }

            additionalArgs += $" --minimum-expected-tests {minimumExpectedTests}";

            var ignoreExitCodePart = GetIgnoreExitCodePart(ignoreExitCodeArg);
            if(ignoreExitCodePart != null)
            {
                additionalArgs += $" {ignoreExitCodePart}";
            }

            var configurationPath = await GetConfigurationPathAsync(tUnitCoverageProject, configurationPathArgument, cancellationToken);
            return new TUnitSettings(tUnitCoverageProject.ExePath, configurationPath, coberturaPath, additionalArgs);
        }

        private string GetIgnoreExitCodePart(string ignoreExitCodeArg)
        {
            var ignoreExitCodeString = GetIgnoreExitCodeString(ignoreExitCodeArg);
            var ignoredExitCodes = GetIgnoredExitCodes(ignoreExitCodeString);
            if(!ignoredExitCodes.Contains(2) && fccRunWhenTestsFail)
            {
                ignoredExitCodes.Add(2);
            }

            if (ignoredExitCodes.Any())
            {
                return $"--ignore-exit-code {string.Join(";", ignoredExitCodes)}";
            }

            return null;
        }

        private string GetIgnoreExitCodeString(string ignoreExitCodesArg)
        {
            // todo environment variables TESTINGPLATFORM_EXITCODE_IGNORE - what takes precedence
            return ignoreExitCodesArg ?? "";
        }

        private List<int> GetIgnoredExitCodes(string exitCodes)
        {
            try
            {
                var codes = exitCodes.Split(';');
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

                var configurationOrRunSettingsElement = xmlUtils.Load(configurationPathArgument);
                var name = configurationOrRunSettingsElement.Name.LocalName;
                if (name == "Configuration") return configurationPathArgument;
                if (name == "RunSettings")
                {
                    var configurationElement = runSettingsToConfiguration.ConvertToConfiguration(configurationOrRunSettingsElement);
                    if (configurationElement != null)
                    {
                        return WriteConfiguration(tUnitCoverageProject, xmlUtils.Serialize(configurationElement));
                    }
                }
            }

            return await WriteFCCConfigurationAsync(tUnitCoverageProject, cancellationToken);
        }

        private async Task<string> WriteFCCConfigurationAsync(ITUnitCoverageProject tUnitCoverageProject, CancellationToken cancellationToken)
        {
            var configuration = await tUnitCoverageProject.GetConfigurationAsync(cancellationToken);
            return WriteConfiguration(tUnitCoverageProject, configuration);
        }

        private string WriteConfiguration(ITUnitCoverageProject tUnitCoverageProject, string configuration)
        {
            var coverageProject = tUnitCoverageProject.CoverageProject;
            var configurationPath = Path.Combine(coverageProject.CoverageOutputFolder, coverageProject.Id.ToString() + "config.xml");
            fileUtil.WriteAllText(configurationPath, configuration);
            return configurationPath;
        }

        private static string GetCoberturaPath(ITUnitCoverageProject tUnitCoverageProject)
        {
            var coverageProject = tUnitCoverageProject.CoverageProject;
            return Path.Combine(coverageProject.CoverageOutputFolder, coverageProject.Id.ToString() + "coverage.xml");
        }

    }


}
