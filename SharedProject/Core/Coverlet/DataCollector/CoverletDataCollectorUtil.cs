using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using FineCodeCoverage.Core.Coverlet;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Output;
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Engine.Coverlet
{
    [Export(typeof(ICoverletDataCollectorUtil))]
    internal class CoverletDataCollectorUtil : ICoverletDataCollectorUtil
    {
        private readonly IFileUtil _fileUtil;
        private readonly IRunSettingsCoverletConfigurationFactory _runSettingsCoverletConfigurationFactory;
        private readonly ILogger _logger;
        private readonly IProcessUtil _processUtil;
        private readonly IDataCollectorSettingsBuilderFactory _dataCollectorSettingsBuilderFactory;
        private readonly ICoverletDataCollectorGeneratedCobertura _coverletDataCollectorGeneratedCobertura;
        private readonly IProcessResponseProcessor _processResponseProcessor;
        private readonly IToolUnzipper _toolUnzipper;
        private readonly IVsBuildFCCSettingsProvider _vsBuildFCCSettingsProvider;

        // for tests
        internal IRunSettingsCoverletConfiguration _runSettingsCoverletConfiguration;
        internal ICoverageProject _coverageProject;
        private const string LogPrefix = "Coverlet Collector Run";
        private const string ZipPrefix = "coverlet.collector";
        private const string ZipDirectoryName = "coverletCollector";

        internal string TestAdapterPathArg { get; set; }

        [ImportingConstructor]
        public CoverletDataCollectorUtil(
            IFileUtil fileUtil,
            IRunSettingsCoverletConfigurationFactory runSettingsCoverletConfigurationFactory,
            ILogger logger,
            IProcessUtil processUtil,
            IDataCollectorSettingsBuilderFactory dataCollectorSettingsBuilderFactory,
            ICoverletDataCollectorGeneratedCobertura coverletDataCollectorGeneratedCobertura,
            IProcessResponseProcessor processResponseProcessor,
            IToolUnzipper toolUnzipper,
            IVsBuildFCCSettingsProvider vsBuildFCCSettingsProvider
            )
        {
            _fileUtil = fileUtil;
            _runSettingsCoverletConfigurationFactory = runSettingsCoverletConfigurationFactory;
            _logger = logger;
            _processUtil = processUtil;
            _dataCollectorSettingsBuilderFactory = dataCollectorSettingsBuilderFactory;
            _coverletDataCollectorGeneratedCobertura = coverletDataCollectorGeneratedCobertura;
            _processResponseProcessor = processResponseProcessor;
            _toolUnzipper = toolUnzipper;
            _vsBuildFCCSettingsProvider = vsBuildFCCSettingsProvider;
        }

        private bool? GetUseDataCollectorFromProjectFile()
        {
            bool? useDataCollector = null;
            XElement root = _coverageProject.ProjectFileXElement;
            IEnumerable<XElement> propertyGroups = root.Elements().Where(el => el.Name.LocalName == "PropertyGroup");
            foreach (XElement propertyGroup in propertyGroups)
            {
                useDataCollector = UseDataCollector(propertyGroup);
                if (useDataCollector.HasValue)
                {
                    break;
                }
            }

            return useDataCollector;
        }

        private static bool? UseDataCollector(XElement xElement)
        {
            XElement useDataCollectorElement = xElement.Elements().FirstOrDefault(ig => ig.Name.LocalName == "UseDataCollector");
            if (useDataCollectorElement == null)
            {
                return null;
            }

            string useDataCollectorValue = useDataCollectorElement.Value.ToLower().Trim();
            return useDataCollectorValue == "true" || useDataCollectorValue.Length == 0;
        }

        private async Task<bool?> GetUseDataCollectorElementAsync()
        {
            bool? useDataCollector = GetUseDataCollectorFromProjectFile();
            if (!useDataCollector.HasValue)
            {
                XElement importedSettings = await _vsBuildFCCSettingsProvider.GetSettingsAsync(_coverageProject.Id);
                if (importedSettings != null)
                {
                    useDataCollector = UseDataCollector(importedSettings);
                }
            }

            return useDataCollector;
        }

        private async Task<bool> OverriddenFromProjectFileAsync()
        {
            bool? useDataCollectorFromProjectFile = await GetUseDataCollectorElementAsync();
            return useDataCollectorFromProjectFile == false;
        }

        private async Task<bool> HasSetUseDataCollectorInProjectFileAsync()
        {
            bool? useDataCollector = await GetUseDataCollectorElementAsync();
            return useDataCollector == true;
        }

        public async Task<bool> CanUseDataCollectorAsync(ICoverageProject coverageProject)
        {
            _runSettingsCoverletConfiguration = _runSettingsCoverletConfigurationFactory.Create();
            _coverageProject = coverageProject;

            if (coverageProject.RunSettingsFile != null)
            {
                string runSettingsXml = _fileUtil.ReadAllText(coverageProject.RunSettingsFile);

                _ = _runSettingsCoverletConfiguration.Read(runSettingsXml);
                switch (_runSettingsCoverletConfiguration.CoverletDataCollectorState)
                {
                    case CoverletDataCollectorState.Disabled:
                        return false;
                    case CoverletDataCollectorState.Enabled:
                        return !await OverriddenFromProjectFileAsync();
                }
            }

            return await HasSetUseDataCollectorInProjectFileAsync();
        }

        private string GetSettings()
        {
            IDataCollectorSettingsBuilder dataCollectorSettingsBuilder = _dataCollectorSettingsBuilderFactory.Create();
            dataCollectorSettingsBuilder
                .Initialize(_coverageProject.Settings.RunSettingsOnly, _coverageProject.RunSettingsFile, Path.Combine(_coverageProject.CoverageOutputFolder, "FCC.runsettings"));

            // command arguments
            dataCollectorSettingsBuilder
                .WithProjectDll(_coverageProject.TestDllFile);
            dataCollectorSettingsBuilder
                .WithBlame();
            dataCollectorSettingsBuilder
                .WithNoLogo();
            dataCollectorSettingsBuilder
                .WithDiagnostics($"{_coverageProject.CoverageOutputFolder}/diagnostics.log");

            dataCollectorSettingsBuilder
                .WithResultsDirectory(_coverageProject.CoverageOutputFolder);

            string[] projectExcludes = _coverageProject.ExcludedReferencedProjects.Select(erp => $"[{erp.AssemblyName}]*").ToArray();
            if (_coverageProject.Settings.Exclude != null)
            {
                projectExcludes = projectExcludes.Concat(SanitizeExcludesOrIncludes(_coverageProject.Settings.Exclude)).ToArray();
            }

            // DataCollector Configuration
            dataCollectorSettingsBuilder
                .WithExclude(projectExcludes, _runSettingsCoverletConfiguration.Exclude);
            dataCollectorSettingsBuilder
                .WithExcludeByFile(
                    SanitizeExcludesOrIncludes(_coverageProject.Settings.ExcludeByFile),
                    _runSettingsCoverletConfiguration.ExcludeByFile);
            dataCollectorSettingsBuilder
                .WithExcludeByAttribute(
                    SanitizeExcludesOrIncludes(_coverageProject.Settings.ExcludeByAttribute),
                    _runSettingsCoverletConfiguration.ExcludeByAttribute);

            IEnumerable<string> projectIncludes = _coverageProject.IncludedReferencedProjects.Select(irp => $"[{irp.AssemblyName}]*");
            if (_coverageProject.Settings.Include != null)
            {
                projectIncludes = projectIncludes.Concat(SanitizeExcludesOrIncludes(_coverageProject.Settings.Include));
            }

            if (_coverageProject.Settings.IncludeTestAssembly && projectIncludes.Any())
            {
                projectIncludes = projectIncludes.Concat(new string[] { $"[{_coverageProject.ProjectName}]*" });
            }

            dataCollectorSettingsBuilder
                .WithInclude(projectIncludes.ToArray(), _runSettingsCoverletConfiguration.Include);
            dataCollectorSettingsBuilder
                .WithIncludeTestAssembly(_coverageProject.Settings.IncludeTestAssembly, _runSettingsCoverletConfiguration.IncludeTestAssembly);

            dataCollectorSettingsBuilder
                .WithIncludeDirectory(_runSettingsCoverletConfiguration.IncludeDirectory);
            dataCollectorSettingsBuilder
                .WithSingleHit(_runSettingsCoverletConfiguration.SingleHit);
            dataCollectorSettingsBuilder
                .WithUseSourceLink(_runSettingsCoverletConfiguration.UseSourceLink);
            dataCollectorSettingsBuilder
                .WithSkipAutoProps(_runSettingsCoverletConfiguration.SkipAutoProps);

            return dataCollectorSettingsBuilder
                .Build();
        }

        private static string[] SanitizeExcludesOrIncludes(string[] excludesOrIncludes)
            => (excludesOrIncludes ?? Array.Empty<string>())
                .Where(x => x != null)
                .Select(x => x.Trim(' ', '\'', '\"'))
                .Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

        private async Task<string> GetTestAdapterPathArgAsync()
        {
            if (!string.IsNullOrWhiteSpace(_coverageProject.Settings.CoverletCollectorDirectoryPath))
            {
                string directoryPath = _coverageProject.Settings.CoverletCollectorDirectoryPath.Trim();
                if (Directory.Exists(directoryPath))
                {
                    await _logger.LogAsync($"Using custom coverlet data collector : {directoryPath}");
                    return $@"""{directoryPath}""";
                }
            }

            return TestAdapterPathArg;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            string settings = GetSettings();

            await LogRunAsync(settings);

            ExecuteResponse result = await _processUtil
            .ExecuteAsync(
                new ExecuteRequest
                {
                    FilePath = "dotnet",
                    Arguments = $@"test --collect:""XPlat Code Coverage"" {settings} --test-adapter-path {await GetTestAdapterPathArgAsync()}",
                    WorkingDirectory = _coverageProject.ProjectOutputFolder,
                },
                cancellationToken
            );

            // this is how coverlet console determines exit code
            // https://github.com/coverlet-coverage/coverlet/blob/ac0e0fad2f0301a3fe9a3de9f8cdb32f406ce6d8/src/coverlet.console/Program.cs
            // https://github.com/coverlet-coverage/coverlet/issues/388

            // vstest
            // https://github.com/microsoft/vstest/blob/34fa5b59661c3d87c849e81fa5be68e3dec90b76/src/vstest.console/CommandLine/Executor.cs#L146

            // dotnet
            // https://github.com/dotnet/sdk/blob/936935f18c3540ed77c97e392780a9dd82aca441/src/Cli/dotnet/commands/dotnet-test/Program.cs#L86

            // test failure has exit code 1
            _ = await _processResponseProcessor.ProcessAsync(
                result,
                code => code == 0 || code == 1,
                true,
                $"{GetLogTitle()} - Output",
                () => _coverletDataCollectorGeneratedCobertura.CorrectPath(
                    _coverageProject.CoverageOutputFolder, _coverageProject.CoverageOutputFile));
        }

        private string GetLogTitle() => $"{LogPrefix} ({_coverageProject.ProjectName})";

        internal string LogRunMessage(string coverletSettings)
            => $"{GetLogTitle()} Arguments {Environment.NewLine}{string.Join($"{Environment.NewLine}", coverletSettings)}";

        private Task LogRunAsync(string coverletSettings) => _logger.LogAsync(LogRunMessage(coverletSettings));

        public void Initialize(string appDataFolder, CancellationToken cancellationToken)
        {
            string zipDestination = _toolUnzipper.EnsureUnzipped(appDataFolder, ZipDirectoryName, ZipPrefix, cancellationToken);
            string testAdapterPath = Path.Combine(zipDestination, "build", "netstandard2.0");
            TestAdapterPathArg = $@"""{testAdapterPath}""";
        }
    }
}
