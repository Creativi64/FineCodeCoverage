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
            XElement root = this.coverageProject.ProjectFileXElement;
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
            if (useDataCollectorElement != null)
            {
                string useDataCollectorValue = useDataCollectorElement.Value.ToLower().Trim();
                return useDataCollectorValue == "true" || useDataCollectorValue.Length == 0;
            }

            return null;
        }

        private async Task<bool?> GetUseDataCollectorElementAsync()
        {
            bool? useDataCollector = this.GetUseDataCollectorFromProjectFile();
            if (!useDataCollector.HasValue)
            {
                XElement importedSettings = await this.vsBuildFCCSettingsProvider.GetSettingsAsync(this.coverageProject.Id);
                if (importedSettings != null)
                {
                    useDataCollector = UseDataCollector(importedSettings);
                }
            }

            return useDataCollector;
        }

        private async Task<bool> OverriddenFromProjectFileAsync()
        {
            bool? useDataCollectorFromProjectFile = await this.GetUseDataCollectorElementAsync();
            return useDataCollectorFromProjectFile == false;
        }

        private async Task<bool> HasSetUseDataCollectorInProjectFileAsync()
        {
            bool? useDataCollector = await this.GetUseDataCollectorElementAsync();
            return useDataCollector == true;
        }

        public async Task<bool> CanUseDataCollectorAsync(ICoverageProject coverageProject)
        {
            this.runSettingsCoverletConfiguration = this.runSettingsCoverletConfigurationFactory.Create();
            this.coverageProject = coverageProject;

            if (coverageProject.RunSettingsFile != null)
            {
                string runSettingsXml = this.fileUtil.ReadAllText(coverageProject.RunSettingsFile);

                _ = this.runSettingsCoverletConfiguration.Read(runSettingsXml);
                switch (this.runSettingsCoverletConfiguration.CoverletDataCollectorState)
                {
                    case CoverletDataCollectorState.Disabled:
                        return false;
                    case CoverletDataCollectorState.Enabled:
                        return !await this.OverriddenFromProjectFileAsync();
                }
            }

            return await this.HasSetUseDataCollectorInProjectFileAsync();
        }

        private string GetSettings()
        {
            IDataCollectorSettingsBuilder dataCollectorSettingsBuilder = this.dataCollectorSettingsBuilderFactory.Create();
            dataCollectorSettingsBuilder
                .Initialize(this.coverageProject.Settings.RunSettingsOnly, this.coverageProject.RunSettingsFile, Path.Combine(this.coverageProject.CoverageOutputFolder, "FCC.runsettings"));

            // command arguments
            dataCollectorSettingsBuilder
                .WithProjectDll(this.coverageProject.TestDllFile);
            dataCollectorSettingsBuilder
                .WithBlame();
            dataCollectorSettingsBuilder
                .WithNoLogo();
            dataCollectorSettingsBuilder
                .WithDiagnostics($"{this.coverageProject.CoverageOutputFolder}/diagnostics.log");

            dataCollectorSettingsBuilder
                .WithResultsDirectory(this.coverageProject.CoverageOutputFolder);

            string[] projectExcludes = this.coverageProject.ExcludedReferencedProjects.Select(erp => $"[{erp.AssemblyName}]*").ToArray();
            if (this.coverageProject.Settings.Exclude != null)
            {
                projectExcludes = projectExcludes.Concat(SanitizeExcludesOrIncludes(this.coverageProject.Settings.Exclude)).ToArray();
            }

            //DataCollector Configuration
            dataCollectorSettingsBuilder
                .WithExclude(projectExcludes, this.runSettingsCoverletConfiguration.Exclude);
            dataCollectorSettingsBuilder
                .WithExcludeByFile(
                    SanitizeExcludesOrIncludes(this.coverageProject.Settings.ExcludeByFile),
                    this.runSettingsCoverletConfiguration.ExcludeByFile);
            dataCollectorSettingsBuilder
                .WithExcludeByAttribute(
                    SanitizeExcludesOrIncludes(this.coverageProject.Settings.ExcludeByAttribute),
                    this.runSettingsCoverletConfiguration.ExcludeByAttribute);

            IEnumerable<string> projectIncludes = this.coverageProject.IncludedReferencedProjects.Select(irp => $"[{irp.AssemblyName}]*");
            if (this.coverageProject.Settings.Include != null)
            {
                projectIncludes = projectIncludes.Concat(SanitizeExcludesOrIncludes(this.coverageProject.Settings.Include));
            }

            if (this.coverageProject.Settings.IncludeTestAssembly && projectIncludes.Any())
            {
                projectIncludes = projectIncludes.Concat(new string[] { $"[{this.coverageProject.ProjectName}]*" });
            }

            dataCollectorSettingsBuilder
                .WithInclude(projectIncludes.ToArray(), this.runSettingsCoverletConfiguration.Include);
            dataCollectorSettingsBuilder
                .WithIncludeTestAssembly(this.coverageProject.Settings.IncludeTestAssembly, this.runSettingsCoverletConfiguration.IncludeTestAssembly);

            dataCollectorSettingsBuilder
                .WithIncludeDirectory(this.runSettingsCoverletConfiguration.IncludeDirectory);
            dataCollectorSettingsBuilder
                .WithSingleHit(this.runSettingsCoverletConfiguration.SingleHit);
            dataCollectorSettingsBuilder
                .WithUseSourceLink(this.runSettingsCoverletConfiguration.UseSourceLink);
            dataCollectorSettingsBuilder
                .WithSkipAutoProps(this.runSettingsCoverletConfiguration.SkipAutoProps);

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
            if (!string.IsNullOrWhiteSpace(this.coverageProject.Settings.CoverletCollectorDirectoryPath))
            {
                string directoryPath = this.coverageProject.Settings.CoverletCollectorDirectoryPath.Trim();
                if (Directory.Exists(directoryPath))
                {
                    await this.logger.LogAsync($"Using custom coverlet data collector : {directoryPath}");
                    return $@"""{directoryPath}""";
                }
            }

            return this.TestAdapterPathArg;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            string settings = this.GetSettings();

            await this.LogRunAsync(settings);

            ExecuteResponse result = await this.processUtil
            .ExecuteAsync(new ExecuteRequest
            {
                FilePath = "dotnet",
                Arguments = $@"test --collect:""XPlat Code Coverage"" {settings} --test-adapter-path {await this.GetTestAdapterPathArgAsync()}",
                WorkingDirectory = this.coverageProject.ProjectOutputFolder
            }, cancellationToken);
            // this is how coverlet console determines exit code
            // https://github.com/coverlet-coverage/coverlet/blob/ac0e0fad2f0301a3fe9a3de9f8cdb32f406ce6d8/src/coverlet.console/Program.cs
            // https://github.com/coverlet-coverage/coverlet/issues/388

            // vstest
            // https://github.com/microsoft/vstest/blob/34fa5b59661c3d87c849e81fa5be68e3dec90b76/src/vstest.console/CommandLine/Executor.cs#L146

            // dotnet
            // https://github.com/dotnet/sdk/blob/936935f18c3540ed77c97e392780a9dd82aca441/src/Cli/dotnet/commands/dotnet-test/Program.cs#L86

            // test failure has exit code 1 
            _ = await this.processResponseProcessor.ProcessAsync(
                result,
                code => code == 0 || code == 1,
                true,
                $"{this.GetLogTitle()} - Output",
                () => this.coverletDataCollectorGeneratedCobertura.CorrectPath(
                    this.coverageProject.CoverageOutputFolder, this.coverageProject.CoverageOutputFile));

        }
        private string GetLogTitle() => $"{LogPrefix} ({this.coverageProject.ProjectName})";

        internal string LogRunMessage(string coverletSettings)
            => $"{this.GetLogTitle()} Arguments {Environment.NewLine}{string.Join($"{Environment.NewLine}", coverletSettings)}";

        private Task LogRunAsync(string coverletSettings) => this.logger.LogAsync(this.LogRunMessage(coverletSettings));

        public void Initialize(string appDataFolder, CancellationToken cancellationToken)
        {
            string zipDestination = this.toolUnzipper.EnsureUnzipped(appDataFolder, zipDirectoryName, zipPrefix, cancellationToken);
            string testAdapterPath = Path.Combine(zipDestination, "build", "netstandard2.0");
            this.TestAdapterPathArg = $@"""{testAdapterPath}""";
        }
    }
}