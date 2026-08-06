using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Collection.CoverageProjectManagement;
using FineCodeCoverage.Options.Base;
using FineCodeCoverage.Options.Run;
using FineCodeCoverage.Utilities.Wrappers;
using FineCodeCoverage.Utilities.Xml;
using FineCodeCoverage.VSAbstractions.OutputWindow;

namespace FineCodeCoverage.Collection.TestingPlatform.TUnit
{
    [Export(typeof(ITUnitSettingsProvider))]
    internal sealed class TUnitSettingsProvider : ITUnitSettingsProvider
    {
        private const string DefaultTestResultsFolderName = "TestResults";

        private static readonly HashSet<string> s_reportGeneratorReadableFormats
            = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase) { "cobertura", "xml" };

        private readonly IFileUtil _fileUtil;
        private readonly IXmlUtils _xmlUtils;
        private readonly IRunSettingsToConfiguration _runSettingsToConfiguration;
        private readonly IOptionsProvider<RunOptions> _runOptionsProvider;
        private readonly IEnvironment _environment;
        private readonly ILogger _logger;
        private int _fccRunWhenTestsExceed;
        private bool _fccRunWhenTestsFail;

        [ImportingConstructor]
        public TUnitSettingsProvider(
            IFileUtil fileUtil,
            IXmlUtils xmlUtils,
            IRunSettingsToConfiguration runSettingsToConfiguration,
            IOptionsProvider<RunOptions> runOptionsProvider,
            IEnvironment environment,
            ILogger logger)
        {
            _fileUtil = fileUtil;
            _xmlUtils = xmlUtils;
            _runSettingsToConfiguration = runSettingsToConfiguration;
            _runOptionsProvider = runOptionsProvider;
            _environment = environment;
            _logger = logger;
            TakeFCCOptions(runOptionsProvider.Get());
            _runOptionsProvider.OptionsChanged += TakeFCCOptions;
        }

        private void TakeFCCOptions(RunOptions appOptions)
        {
            _fccRunWhenTestsExceed = appOptions.RunWhenTestsExceed;
            _fccRunWhenTestsFail = appOptions.RunWhenTestsFail;
        }

        public async Task<TUnitSettings> ProvideAsync(ITUnitCoverageProject tUnitCoverageProject, CancellationToken cancellationToken)
        {
            // this empties the coverage output folder, so a run that does not produce coverage
            // can never present the previous run's report
            _ = await tUnitCoverageProject.CoverageProject.PrepareForCoverageAsync(cancellationToken, false);
            string coberturaPath = GetCoberturaPath(tUnitCoverageProject);
            CommandLineParseResult commandLineParseResult = tUnitCoverageProject.CommandLineParseResult;

            // todo commandLineParseResult.HasError
            string configurationPathArgument = null;
            string userOutputPath = null;
            string userOutputFormat = null;
            var additionalArgsStringBuilder = new StringBuilder();
            string ignoreExitCodeArg = null;
            int? minimumExpectedTests = null;
            foreach (CommandLineParseOption option in commandLineParseResult.Options)
            {
                switch (option.Name)
                {
                    case "coverage":
                        // nothing to fill in - the user has already asked for coverage
                        break;
                    case "coverage-output":
                        userOutputPath = ResolveUserOutputPath(tUnitCoverageProject, Unquote(option.Arguments.FirstOrDefault()));
                        break;
                    case "coverage-output-format":
                        userOutputFormat = await ProvideOutputFormatAsync(Unquote(option.Arguments.FirstOrDefault()));
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

            AddToAdditionalArgs(GetMinimumExpectedTestsPart(minimumExpectedTests));
            AddToAdditionalArgs(GetIgnoreExitCodePart(ignoreExitCodeArg));

            (string configurationPath, bool userSuppliedConfiguration) =
                await GetConfigurationPathAsync(tUnitCoverageProject, configurationPathArgument, cancellationToken);
            return new TUnitSettings(
                tUnitCoverageProject.ExePath,
                configurationPath,
                userSuppliedConfiguration,
                coberturaPath,
                userOutputPath,
                userOutputFormat,
                additionalArgsStringBuilder.ToString());

            bool ConfigurationPathArgExists(string pathArg)
            {
                pathArg = Unquote(pathArg);
                return _fileUtil.Exists(pathArg);
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

        private static string Unquote(string arg)
            => arg?.Replace("\"", string.Empty).Replace("'", string.Empty);

        /*
            Microsoft.Testing.Platform resolves a --coverage-output that is not rooted relative to
            --results-directory, which defaults to TestResults in the test application directory.
        */
        private static string ResolveUserOutputPath(ITUnitCoverageProject tUnitCoverageProject, string userOutputPath)
        {
            if (string.IsNullOrWhiteSpace(userOutputPath))
            {
                return null;
            }

            return Path.IsPathRooted(userOutputPath)
                ? userOutputPath
                : Path.Combine(
                    tUnitCoverageProject.CoverageProject.ProjectOutputFolder,
                    DefaultTestResultsFolderName,
                    userOutputPath);
        }

        /*
            Microsoft.Testing.Platform supports coverage, xml and cobertura.  ReportGenerator can read
            cobertura and the dynamic code coverage xml but not the binary coverage format.
        */
        private async Task<string> ProvideOutputFormatAsync(string userOutputFormat)
        {
            if (string.IsNullOrWhiteSpace(userOutputFormat))
            {
                return null;
            }

            if (s_reportGeneratorReadableFormats.Contains(userOutputFormat))
            {
                return userOutputFormat.ToLowerInvariant();
            }

            await _logger.LogAsync(
                $"Coverage output format '{userOutputFormat}' cannot be read by ReportGenerator.  Using {TUnitSettings.DefaultOutputFormat}.");
            return null;
        }

        private string GetMinimumExpectedTestsPart(int? minimumExpectedTestsArg)
        {
            // non zero positive integer
            if (!minimumExpectedTestsArg.HasValue && _fccRunWhenTestsExceed > 1)
            {
                minimumExpectedTestsArg = _fccRunWhenTestsExceed - 1;
            }

            return minimumExpectedTestsArg.HasValue ? $"--minimum-expected-tests {minimumExpectedTestsArg}" : null;
        }

        private string GetIgnoreExitCodePart(string ignoreExitCodeArg)
        {
            string ignoreExitCodeString = GetIgnoreExitCodeString(ignoreExitCodeArg);
            List<int> ignoredExitCodes = GetIgnoredExitCodes(ignoreExitCodeString);
            if (!ignoredExitCodes.Contains(2) && _fccRunWhenTestsFail)
            {
                ignoredExitCodes.Add(2);
            }

            return ignoredExitCodes.Count != 0 ? $"--ignore-exit-code {string.Join(";", ignoredExitCodes)}" : null;
        }

        private string GetIgnoreExitCodeString(string ignoreExitCodesArg)
        {
            string environmentVariableValue = _environment.GetEnvironmentVariable("TESTINGPLATFORM_EXITCODE_IGNORE");
            return environmentVariableValue ?? ignoreExitCodesArg ?? string.Empty;
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

        private async Task<(string Path, bool UserSupplied)> GetConfigurationPathAsync(
            ITUnitCoverageProject tUnitCoverageProject,
            string configurationPathArgument,
            CancellationToken cancellationToken)
        {
            if (configurationPathArgument != null)
            {
                if (tUnitCoverageProject.HasCoverageExtension)
                {
                    return (configurationPathArgument, true);
                }

                System.Xml.Linq.XElement configurationOrRunSettingsElement = _xmlUtils.Load(configurationPathArgument);
                string name = configurationOrRunSettingsElement.Name.LocalName;
                if (name == "Configuration")
                {
                    return (configurationPathArgument, true);
                }

                if (name == "RunSettings")
                {
                    System.Xml.Linq.XElement configurationElement = _runSettingsToConfiguration.ConvertToConfiguration(configurationOrRunSettingsElement);
                    if (configurationElement != null)
                    {
                        return (WriteConfiguration(tUnitCoverageProject, _xmlUtils.Serialize(configurationElement)), true);
                    }
                }
            }

            return (await WriteFCCConfigurationAsync(tUnitCoverageProject, cancellationToken), false);
        }

        private async Task<string> WriteFCCConfigurationAsync(ITUnitCoverageProject tUnitCoverageProject, CancellationToken cancellationToken)
        {
            string configuration = await tUnitCoverageProject.GetConfigurationAsync(cancellationToken);
            return WriteConfiguration(tUnitCoverageProject, configuration);
        }

        private string WriteConfiguration(ITUnitCoverageProject tUnitCoverageProject, string configuration)
        {
            ICoverageProject coverageProject = tUnitCoverageProject.CoverageProject;
            string configurationPath = Path.Combine(coverageProject.CoverageOutputFolder, coverageProject.Id.ToString() + "config.xml");
            _fileUtil.WriteAllText(configurationPath, configuration);
            return configurationPath;
        }

        private static string GetCoberturaPath(ITUnitCoverageProject tUnitCoverageProject)
        {
            ICoverageProject coverageProject = tUnitCoverageProject.CoverageProject;
            return Path.Combine(coverageProject.CoverageOutputFolder, coverageProject.Id.ToString() + "coverage.xml");
        }
    }
}
