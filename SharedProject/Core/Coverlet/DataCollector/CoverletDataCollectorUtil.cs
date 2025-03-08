using System;
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
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Engine.Coverlet
{

    [Export(typeof(ICoverletDataCollectorUtil))]
    internal class CoverletDataCollectorUtil : ICoverletDataCollectorUtil
    {
        private readonly IFileUtil fileUtil;
        private readonly IRunSettingsCoverletConfigurationFactory runSettingsCoverletConfigurationFactory;
        private readonly ILogger logger;
        private readonly IProcessUtil processUtil;
        private readonly IDataCollectorSettingsBuilderFactory dataCollectorSettingsBuilderFactory;
        private readonly ICoverletDataCollectorGeneratedCobertura coverletDataCollectorGeneratedCobertura;
        private readonly IProcessResponseProcessor processResponseProcessor;
        private readonly IToolUnzipper toolUnzipper;
        private readonly IVsBuildFCCSettingsProvider vsBuildFCCSettingsProvider;


        //for tests
        internal IRunSettingsCoverletConfiguration runSettingsCoverletConfiguration;
        internal ICoverageProject coverageProject;
        private const string LogPrefix = "Coverlet Collector Run";
        internal string TestAdapterPathArg { get; set; }
        private const string zipPrefix = "coverlet.collector";
        private const string zipDirectoryName = "coverletCollector";

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
            this.fileUtil = fileUtil;
            this.runSettingsCoverletConfigurationFactory = runSettingsCoverletConfigurationFactory;
            this.logger = logger;
            this.processUtil = processUtil;
            this.dataCollectorSettingsBuilderFactory = dataCollectorSettingsBuilderFactory;
            this.coverletDataCollectorGeneratedCobertura = coverletDataCollectorGeneratedCobertura;
            this.processResponseProcessor = processResponseProcessor;
            this.toolUnzipper = toolUnzipper;
            this.vsBuildFCCSettingsProvider = vsBuildFCCSettingsProvider;
        }

        private bool? GetUseDataCollectorFromProjectFile()
        {
            bool? useDataCollector = null;
            var root = coverageProject.ProjectFileXElement;
            var propertyGroups = root.Elements().Where(el => el.Name.LocalName == "PropertyGroup");
            foreach (var propertyGroup in propertyGroups)
            {
                useDataCollector = UseDataCollector(propertyGroup);
                if (useDataCollector.HasValue)
                {
                    break;
                }

            }
            return useDataCollector;
        }

        private bool? UseDataCollector(XElement xElement)
        {
            var useDataCollectorElement = xElement.Elements().FirstOrDefault(ig => ig.Name.LocalName == "UseDataCollector");
            if (useDataCollectorElement != null)
            {
                var useDataCollectorValue = useDataCollectorElement.Value.ToLower().Trim();
                return useDataCollectorValue == "true" || useDataCollectorValue.Length == 0;
            }
            return null;
        }

        private async Task<bool?> GetUseDataCollectorElementAsync()
        {
            var useDataCollector = GetUseDataCollectorFromProjectFile();
            if (!useDataCollector.HasValue)
            {
                var importedSettings = await vsBuildFCCSettingsProvider.GetSettingsAsync(coverageProject.Id);
                if (importedSettings != null)
                {
                    useDataCollector = UseDataCollector(importedSettings);
                }
            }

            return useDataCollector;
        }

        private async Task<bool> OverriddenFromProjectFileAsync()
        {
            var useDataCollectorFromProjectFile = await GetUseDataCollectorElementAsync();
            if (useDataCollectorFromProjectFile.HasValue)
            {
                return !useDataCollectorFromProjectFile.Value;
            }
            else
            {
                return false;
            }
        }

        private async Task<bool> HasSetUseDataCollectorInProjectFileAsync()
        {
            var useDataCollector = await GetUseDataCollectorElementAsync();
            return useDataCollector == true;
        }

        public async Task<bool> CanUseDataCollectorAsync(ICoverageProject coverageProject)
        {
            runSettingsCoverletConfiguration = runSettingsCoverletConfigurationFactory.Create();
            this.coverageProject = coverageProject;

            if(coverageProject.RunSettingsFile != null)
            {
                var runSettingsXml = fileUtil.ReadAllText(coverageProject.RunSettingsFile);

                runSettingsCoverletConfiguration.Read(runSettingsXml);
                switch (runSettingsCoverletConfiguration.CoverletDataCollectorState)
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
            var dataCollectorSettingsBuilder = dataCollectorSettingsBuilderFactory.Create();
            dataCollectorSettingsBuilder
                .Initialize(coverageProject.Settings.RunSettingsOnly, coverageProject.RunSettingsFile, Path.Combine(coverageProject.CoverageOutputFolder,"FCC.runsettings"));

            // command arguments
            dataCollectorSettingsBuilder
                .WithProjectDll(coverageProject.TestDllFile);
            dataCollectorSettingsBuilder
                .WithBlame();
            dataCollectorSettingsBuilder
                .WithNoLogo();
            dataCollectorSettingsBuilder
                .WithDiagnostics($"{coverageProject.CoverageOutputFolder}/diagnostics.log");

            dataCollectorSettingsBuilder
                .WithResultsDirectory(coverageProject.CoverageOutputFolder);

            string[] projectExcludes = coverageProject.ExcludedReferencedProjects.Select(erp => $"[{erp.AssemblyName}]*").ToArray();
            if(coverageProject.Settings.Exclude != null)
            {
                projectExcludes = projectExcludes.Concat(SanitizeExcludesOrIncludes(coverageProject.Settings.Exclude)).ToArray();
            }

            //DataCollector Configuration
            dataCollectorSettingsBuilder
                .WithExclude(projectExcludes, runSettingsCoverletConfiguration.Exclude);
            dataCollectorSettingsBuilder
                .WithExcludeByFile(
                    SanitizeExcludesOrIncludes(coverageProject.Settings.ExcludeByFile),
                    runSettingsCoverletConfiguration.ExcludeByFile);
            dataCollectorSettingsBuilder
                .WithExcludeByAttribute(
                    SanitizeExcludesOrIncludes(coverageProject.Settings.ExcludeByAttribute),
                    runSettingsCoverletConfiguration.ExcludeByAttribute);

            string[] projectIncludes = coverageProject.IncludedReferencedProjects.Select(irp => $"[{irp.AssemblyName}]*").ToArray();
            if(coverageProject.Settings.Include != null)
            {
                projectIncludes = projectIncludes.Concat(SanitizeExcludesOrIncludes(coverageProject.Settings.Include)).ToArray();
            }

            dataCollectorSettingsBuilder
                .WithInclude(projectIncludes, runSettingsCoverletConfiguration.Include);
            dataCollectorSettingsBuilder
                .WithIncludeTestAssembly(coverageProject.Settings.IncludeTestAssembly, runSettingsCoverletConfiguration.IncludeTestAssembly);

            dataCollectorSettingsBuilder
                .WithIncludeDirectory(runSettingsCoverletConfiguration.IncludeDirectory);
            dataCollectorSettingsBuilder
                .WithSingleHit(runSettingsCoverletConfiguration.SingleHit);
            dataCollectorSettingsBuilder
                .WithUseSourceLink(runSettingsCoverletConfiguration.UseSourceLink);
            dataCollectorSettingsBuilder
                .WithSkipAutoProps(runSettingsCoverletConfiguration.SkipAutoProps);

            return dataCollectorSettingsBuilder
                .Build();

        }

        private string[] SanitizeExcludesOrIncludes(string[] excludesOrIncludes)
        {
            return (excludesOrIncludes ?? new string[0])
                .Where(x => x != null)
                .Select(x => x.Trim(' ', '\'', '\"'))
                .Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        }
        private async Task<string> GetTestAdapterPathArgAsync()
        {
            if (!String.IsNullOrWhiteSpace(coverageProject.Settings.CoverletCollectorDirectoryPath)) {
                var directoryPath = coverageProject.Settings.CoverletCollectorDirectoryPath.Trim();
                if (Directory.Exists(directoryPath))
                {
                    await logger.LogAsync($"Using custom coverlet data collector : {directoryPath}");
                    return $@"""{directoryPath}""";
                }
            }
            return TestAdapterPathArg;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            var settings = GetSettings();

            await LogRunAsync(settings);

            var result = await processUtil
            .ExecuteAsync(new ExecuteRequest
            {
                FilePath = "dotnet",
                Arguments = $@"test --collect:""XPlat Code Coverage"" {settings} --test-adapter-path {await GetTestAdapterPathArgAsync()}",
                WorkingDirectory = coverageProject.ProjectOutputFolder
            }, cancellationToken);
            // this is how coverlet console determines exit code
            // https://github.com/coverlet-coverage/coverlet/blob/ac0e0fad2f0301a3fe9a3de9f8cdb32f406ce6d8/src/coverlet.console/Program.cs
            // https://github.com/coverlet-coverage/coverlet/issues/388

            // vstest
            // https://github.com/microsoft/vstest/blob/34fa5b59661c3d87c849e81fa5be68e3dec90b76/src/vstest.console/CommandLine/Executor.cs#L146

            // dotnet
            // https://github.com/dotnet/sdk/blob/936935f18c3540ed77c97e392780a9dd82aca441/src/Cli/dotnet/commands/dotnet-test/Program.cs#L86

            // test failure has exit code 1 
            await processResponseProcessor.ProcessAsync(
                result,
                code => code == 0 || code == 1,
                true,
                $"{GetLogTitle()} - Output",
                () => coverletDataCollectorGeneratedCobertura.CorrectPath(
                    coverageProject.CoverageOutputFolder, coverageProject.CoverageOutputFile));

        }
        private string GetLogTitle()
        {
            return $"{LogPrefix} ({coverageProject.ProjectName})";
        }
        internal string LogRunMessage(string coverletSettings)
        {
            return $"{GetLogTitle()} Arguments {Environment.NewLine}{string.Join($"{Environment.NewLine}", coverletSettings)}";
        }
        private Task LogRunAsync(string coverletSettings)
        {
            return logger.LogAsync(LogRunMessage(coverletSettings));
        }

        public void Initialize(string appDataFolder,CancellationToken cancellationToken)
        {
            var zipDestination = toolUnzipper.EnsureUnzipped(appDataFolder, zipDirectoryName, zipPrefix, cancellationToken);
            var testAdapterPath = Path.Combine(zipDestination, "build", "netstandard2.0");
            TestAdapterPathArg = $@"""{testAdapterPath}""";
        }
    }
}
