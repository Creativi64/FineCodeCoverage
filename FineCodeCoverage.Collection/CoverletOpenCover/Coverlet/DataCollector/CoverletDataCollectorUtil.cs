using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using FineCodeCoverage.Collection.CoverageProjectManagement;
using FineCodeCoverage.Collection.CoverageProjectManagement.Settings;
using FineCodeCoverage.Collection.CoverletOpenCover.Process;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Output;
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Collection.CoverletOpenCover.Coverlet.DataCollector
{
    [Export(typeof(ICoverletDataCollectorUtil))]
    internal sealed class CoverletDataCollectorUtil : ICoverletDataCollectorUtil
    {
        private const string LogPrefix = "Coverlet Collector Run";
        private const string ZipPrefix = "coverlet.collector";
        private const string ZipDirectoryName = "coverletCollector";
        private readonly IFileUtil _fileUtil;
        private readonly IRunSettingsCoverletConfigurationFactory _runSettingsCoverletConfigurationFactory;
        private readonly ILogger _logger;
        private readonly IProcessUtil _processUtil;
        private readonly IDataCollectorSettingsBuilderFactory _dataCollectorSettingsBuilderFactory;
        private readonly ICoverletDataCollectorGeneratedCobertura _coverletDataCollectorGeneratedCobertura;
        private readonly IProcessResponseProcessor _processResponseProcessor;
        private readonly IToolUnzipper _toolUnzipper;
        private readonly IVsBuildFCCSettingsProvider _vsBuildFCCSettingsProvider;

        // internal properties for tests
        internal string TestAdapterPathArg { get; set; }

        internal IRunSettingsCoverletConfiguration RunSettingsCoverletConfiguration { get; set; }

        internal ICoverageProject CoverageProject { get; set; }

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
            IVsBuildFCCSettingsProvider vsBuildFCCSettingsProvider)
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
            XElement root = CoverageProject.ProjectFileXElement;
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
                XElement importedSettings = await _vsBuildFCCSettingsProvider.GetSettingsAsync(CoverageProject.Id);
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
            RunSettingsCoverletConfiguration = _runSettingsCoverletConfigurationFactory.Create();
            CoverageProject = coverageProject;

            if (coverageProject.RunSettingsFile != null)
            {
                string runSettingsXml = _fileUtil.ReadAllText(coverageProject.RunSettingsFile);

                _ = RunSettingsCoverletConfiguration.Read(runSettingsXml);
                switch (RunSettingsCoverletConfiguration.CoverletDataCollectorState)
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
                .Initialize(CoverageProject.Settings.RunSettingsOnly, CoverageProject.RunSettingsFile, Path.Combine(CoverageProject.CoverageOutputFolder, "FCC.runsettings"));

            // command arguments
            dataCollectorSettingsBuilder
                .WithProjectDll(CoverageProject.TestDllFile);
            dataCollectorSettingsBuilder
                .WithBlame();
            dataCollectorSettingsBuilder
                .WithNoLogo();
            dataCollectorSettingsBuilder
                .WithDiagnostics($"{CoverageProject.CoverageOutputFolder}/diagnostics.log");

            dataCollectorSettingsBuilder
                .WithResultsDirectory(CoverageProject.CoverageOutputFolder);

            string[] projectExcludes = CoverageProject.ExcludedReferencedProjects.Select(erp => $"[{erp.AssemblyName}]*").ToArray();
            if (CoverageProject.Settings.Exclude != null)
            {
                projectExcludes = projectExcludes.Concat(SanitizeExcludesOrIncludes(CoverageProject.Settings.Exclude)).ToArray();
            }

            // DataCollector Configuration
            dataCollectorSettingsBuilder
                .WithExclude(projectExcludes, RunSettingsCoverletConfiguration.Exclude);
            dataCollectorSettingsBuilder
                .WithExcludeByFile(
                    SanitizeExcludesOrIncludes(CoverageProject.Settings.ExcludeByFile),
                    RunSettingsCoverletConfiguration.ExcludeByFile);
            dataCollectorSettingsBuilder
                .WithExcludeByAttribute(
                    SanitizeExcludesOrIncludes(CoverageProject.Settings.ExcludeByAttribute),
                    RunSettingsCoverletConfiguration.ExcludeByAttribute);

            IEnumerable<string> projectIncludes = CoverageProject.IncludedReferencedProjects.Select(irp => $"[{irp.AssemblyName}]*");
            if (CoverageProject.Settings.Include != null)
            {
                projectIncludes = projectIncludes.Concat(SanitizeExcludesOrIncludes(CoverageProject.Settings.Include));
            }

            if (CoverageProject.Settings.IncludeTestAssembly && projectIncludes.Any())
            {
                projectIncludes = projectIncludes.Concat(new string[] { $"[{CoverageProject.ProjectName}]*" });
            }

            dataCollectorSettingsBuilder
                .WithInclude(projectIncludes.ToArray(), RunSettingsCoverletConfiguration.Include);
            dataCollectorSettingsBuilder
                .WithIncludeTestAssembly(CoverageProject.Settings.IncludeTestAssembly, RunSettingsCoverletConfiguration.IncludeTestAssembly);

            dataCollectorSettingsBuilder
                .WithIncludeDirectory(RunSettingsCoverletConfiguration.IncludeDirectory);
            dataCollectorSettingsBuilder
                .WithSingleHit(RunSettingsCoverletConfiguration.SingleHit);
            dataCollectorSettingsBuilder
                .WithUseSourceLink(RunSettingsCoverletConfiguration.UseSourceLink);
            dataCollectorSettingsBuilder
                .WithSkipAutoProps(RunSettingsCoverletConfiguration.SkipAutoProps);

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
            if (!string.IsNullOrWhiteSpace(CoverageProject.Settings.CoverletCollectorDirectoryPath))
            {
                string directoryPath = CoverageProject.Settings.CoverletCollectorDirectoryPath.Trim();
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
                    WorkingDirectory = CoverageProject.ProjectOutputFolder,
                },
                cancellationToken);

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
                    CoverageProject.CoverageOutputFolder, CoverageProject.CoverageOutputFile));
        }

        private string GetLogTitle() => $"{LogPrefix} ({CoverageProject.ProjectName})";

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
