using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.XPath;
using FineCodeCoverage.Core.Initialization;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Impl;
using FineCodeCoverage.Options;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using ILogger = FineCodeCoverage.Output.ILogger;
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage
{
    [Export(typeof(IMsCodeCoverageRunSettingsService))]
    [Export(typeof(IRunSettingsService))]
    [Export(typeof(IAppDataFolderPathDependent))]
    internal class MsCodeCoverageRunSettingsService : IMsCodeCoverageRunSettingsService, IRunSettingsService, IAppDataFolderPathDependent
    {
        public string Name => "Fine Code Coverage MsCodeCoverageRunSettingsService";

        private class UserRunSettingsProjectDetails : IUserRunSettingsProjectDetails
        {
            public ICoverageSettings Settings { get; set; }
            public string CoverageOutputFolder { get; set; }
            public string TestDllFile { get; set; }
            public List<IReferencedProject> ExcludedReferencedProjects { get; set; }
            public List<IReferencedProject> IncludedReferencedProjects { get; set; }
        }

        private class CoverageProjectsByType
        {
            public List<ICoverageProject> All { get; private set; }
            public List<ICoverageProject> RunSettings { get; private set; }
            public List<ICoverageProject> Templated { get; private set; }

            public bool HasTemplated()
            {
                return this.Templated.Any();
            }

            public static async Task<CoverageProjectsByType> CreateAsync(ITestOperation testOperation)
            {
                List<ICoverageProject> coverageProjects = await testOperation.GetCoverageProjectsAsync();
                var coverageProjectsWithRunSettings = coverageProjects.Where(coverageProject => coverageProject.RunSettingsFile != null).ToList();
                var coverageProjectsWithoutRunSettings = coverageProjects.Except(coverageProjectsWithRunSettings).ToList();
                return new CoverageProjectsByType
                {
                    All = coverageProjects,
                    RunSettings = coverageProjectsWithRunSettings,
                    Templated = coverageProjectsWithoutRunSettings
                };
            }
        }

        private readonly IToolUnzipper toolUnzipper;
        private readonly IOptionsProvider<RunOptions> runOptionsProvider;
        private readonly ICoverageToolOutputManager coverageOutputManager;
        private readonly IShimCopier shimCopier;
        private readonly ILogger logger;
        private readonly IFCCEngine fccEngine;

        private const string zipPrefix = "microsoft.codecoverage";
        private const string zipDirectoryName = "msCodeCoverage";

        private const string msCodeCoverageMessage = "Ms code coverage";
        internal Dictionary<string, IUserRunSettingsProjectDetails> userRunSettingsProjectDetailsLookup; // for tests

        private readonly IUserRunSettingsService userRunSettingsService;
        private readonly ITemplatedRunSettingsService templatedRunSettingsService;
        private string fccMsTestAdapterPath;
        private string shimPath;

        private CoverageProjectsByType coverageProjectsByType;
        private bool useMsCodeCoverage;

        internal MsCodeCoverageCollectionStatus collectionStatus; // for tests
        private RunMsCodeCoverage runMsCodeCoverage;

        private bool IsCollecting => this.collectionStatus == MsCodeCoverageCollectionStatus.Collecting;

        [ImportingConstructor]
        public MsCodeCoverageRunSettingsService(
            IToolUnzipper toolUnzipper,
            IOptionsProvider<RunOptions> runOptionsProvider,
            ICoverageToolOutputManager coverageOutputManager,
            IUserRunSettingsService userRunSettingsService,
            ITemplatedRunSettingsService templatedRunSettingsService,
            IShimCopier shimCopier,
            ILogger logger,
            IFCCEngine fccEngine
            )
        {
            this.toolUnzipper = toolUnzipper;
            this.runOptionsProvider = runOptionsProvider;
            this.coverageOutputManager = coverageOutputManager;
            this.shimCopier = shimCopier;
            this.logger = logger;
            this.userRunSettingsService = userRunSettingsService;
            this.templatedRunSettingsService = templatedRunSettingsService;
            this.fccEngine = fccEngine;
        }

        public Task InitializeAsync(string appDataFolderPath, CancellationToken cancellationToken)
        {
            string zipDestination = this.toolUnzipper.EnsureUnzipped(appDataFolderPath, zipDirectoryName, zipPrefix, cancellationToken);
            this.fccMsTestAdapterPath = Path.Combine(zipDestination, "build", "netstandard2.0");
            this.shimPath = Path.Combine(zipDestination, "build", "netstandard2.0", "CodeCoverage", "coreclr", "Microsoft.VisualStudio.CodeCoverage.Shim.dll");
            return Task.CompletedTask;
        }

        #region set up for collection

        public async Task<MsCodeCoverageCollectionStatus> IsCollectingAsync(ITestOperation testOperation)
        {
            await this.InitializeIsCollectingAsync(testOperation);
            if (this.runMsCodeCoverage != RunMsCodeCoverage.No)
            {
                await this.TrySetUpForCollectionAsync(testOperation.SolutionDirectory);
            }

            return this.collectionStatus;
        }

        private async Task TrySetUpForCollectionAsync(string solutionDirectory)
        {
            IUserRunSettingsAnalysisResult analysisResult = await this.TryAnalyseUserRunSettingsAsync();
            if (analysisResult.Ok())
            {
                await this.SetUpForCollectionAsync(
                    analysisResult.ProjectsWithFCCMsTestAdapter,
                    analysisResult.SpecifiedMsCodeCoverage,
                    solutionDirectory
                );
            }
        }

        private async Task SetUpForCollectionAsync(
            List<ICoverageProject> coverageProjectsForShim,
            bool specifiedMsCodeCoverageInRunSettings,
            string solutionDirectory
        )
        {
            await this.PrepareCoverageProjectsAsync();
            this.SetUserRunSettingsProjectDetails();

            await this.GenerateTemplatedRunSettingsIfRequiredAsync(
                specifiedMsCodeCoverageInRunSettings,
                coverageProjectsForShim,
                solutionDirectory
            );
            this.CopyShimWhenCollecting(coverageProjectsForShim);
        }

        private Task InitializeIsCollectingAsync(ITestOperation testOperation)
        {
            this.runMsCodeCoverage = this.runOptionsProvider.Get().RunMsCodeCoverage;
            this.useMsCodeCoverage = this.runMsCodeCoverage == RunMsCodeCoverage.Yes;
            this.userRunSettingsProjectDetailsLookup = null;
            return this.CleanUpAsync(testOperation);
        }

        private async Task<IUserRunSettingsAnalysisResult> TryAnalyseUserRunSettingsAsync()
        {
            IUserRunSettingsAnalysisResult analysisResult = null;
            try
            {
                analysisResult = await this.AnalyseUserRunSettingsAsync();
            }
            catch (Exception exc)
            {
                await this.ExceptionAnalysingUserRunSettingsAsync(exc);
            }

            return analysisResult;
        }

        private Task ExceptionAnalysingUserRunSettingsAsync(Exception exc)
        {
            this.collectionStatus = MsCodeCoverageCollectionStatus.Error;
            return this.logger.LogAsync("Exception analysing runsettings files", exc.ToString());
        }

        private async Task<IUserRunSettingsAnalysisResult> AnalyseUserRunSettingsAsync()
        {
            IUserRunSettingsAnalysisResult analysisResult = this.userRunSettingsService.Analyse(
                    this.coverageProjectsByType.RunSettings,
                    this.useMsCodeCoverage,
                    this.fccMsTestAdapterPath
                );

            if (analysisResult.Suitable)
            {
                await this.CollectingIfUserRunSettingsOnlyAsync();
            }

            return analysisResult;
        }

        private async Task GenerateTemplatedRunSettingsIfRequiredAsync(
            bool runSettingsSpecifiedMsCodeCoverage,
            List<ICoverageProject> coverageProjectsForShim,
            string solutionDirectory
        )
        {
            if (this.ShouldGenerateTemplatedRunSettings(runSettingsSpecifiedMsCodeCoverage))
            {
                await this.GenerateTemplatedRunSettingsAsync(coverageProjectsForShim, solutionDirectory);
            }
        }
        private async Task GenerateTemplatedRunSettingsAsync(
            List<ICoverageProject> coverageProjectsForShim,
            string solutionDirectory
        )
        {
            IProjectRunSettingsFromTemplateResult generationResult = await this.templatedRunSettingsService.GenerateAsync(
                this.coverageProjectsByType.Templated,
                solutionDirectory,
                this.fccMsTestAdapterPath
            );

            await this.ProcessTemplateGenerationResultAsync(generationResult, coverageProjectsForShim);
        }


        private bool ShouldGenerateTemplatedRunSettings(bool runSettingsSpecifiedMsCodeCoverage)
        {
            return this.coverageProjectsByType.HasTemplated() && (this.useMsCodeCoverage || runSettingsSpecifiedMsCodeCoverage);
        }

        private async Task ProcessTemplateGenerationResultAsync(IProjectRunSettingsFromTemplateResult generationResult, List<ICoverageProject> coverageProjectsForShim)
        {
            if (generationResult.ExceptionReason == null)
            {
                await this.CollectingWithTemplateAsync(generationResult, coverageProjectsForShim);
            }
            else
            {
                IExceptionReason exceptionReason = generationResult.ExceptionReason;
                await this.logger.LogAsync(exceptionReason.Reason, exceptionReason.Exception.ToString());
                this.collectionStatus = MsCodeCoverageCollectionStatus.Error;
            }
        }

        private async Task CollectingWithTemplateAsync(IProjectRunSettingsFromTemplateResult generationResult, List<ICoverageProject> coverageProjectsForShim)
        {
            coverageProjectsForShim.AddRange(generationResult.CoverageProjectsWithFCCMsTestAdapter);
            string leadingMessage = generationResult.CustomTemplatePaths.Any() ? $"{msCodeCoverageMessage} - custom template paths" : msCodeCoverageMessage;
            IEnumerable<string> loggerMessages = new List<string> { leadingMessage }.Concat(generationResult.CustomTemplatePaths.Distinct());
            await this.logger.LogAsync(loggerMessages);
            this.collectionStatus = MsCodeCoverageCollectionStatus.Collecting;
        }

        private async Task CollectingIfUserRunSettingsOnlyAsync()
        {
            if (!this.coverageProjectsByType.HasTemplated())
            {
                this.collectionStatus = MsCodeCoverageCollectionStatus.Collecting;
                await this.logger.LogAsync($"{msCodeCoverageMessage} with user runsettings");
            }
        }

        private void CopyShimWhenCollecting(List<ICoverageProject> coverageProjectsForShim)
        {
            if (this.IsCollecting)
            {
                this.shimCopier.Copy(this.shimPath, coverageProjectsForShim);
            }
        }

        private async Task PrepareCoverageProjectsAsync()
        {
            await this.coverageOutputManager.SetProjectCoverageOutputFolderAsync(this.coverageProjectsByType.All);
            foreach (ICoverageProject coverageProject in this.coverageProjectsByType.All)
            {
                _ = await coverageProject.PrepareForCoverageAsync(CancellationToken.None, false);
            }
        }

        private void SetUserRunSettingsProjectDetails()
        {
            this.userRunSettingsProjectDetailsLookup = new Dictionary<string, IUserRunSettingsProjectDetails>();
            foreach (ICoverageProject coverageProjectWithRunSettings in this.coverageProjectsByType.RunSettings)
            {
                var userRunSettingsProjectDetails = new UserRunSettingsProjectDetails
                {
                    Settings = coverageProjectWithRunSettings.Settings,
                    CoverageOutputFolder = coverageProjectWithRunSettings.CoverageOutputFolder,
                    TestDllFile = coverageProjectWithRunSettings.TestDllFile,
                    ExcludedReferencedProjects = coverageProjectWithRunSettings.ExcludedReferencedProjects,
                    IncludedReferencedProjects = coverageProjectWithRunSettings.IncludedReferencedProjects
                };
                this.userRunSettingsProjectDetailsLookup.Add(coverageProjectWithRunSettings.TestDllFile, userRunSettingsProjectDetails);
            }
        }

        #endregion

        #region IRunSettingsService
        public IXPathNavigable AddRunSettings(IXPathNavigable inputRunSettingDocument, IRunSettingsConfigurationInfo configurationInfo, Microsoft.VisualStudio.TestWindow.Extensibility.ILogger log)
        {
            if (configurationInfo.IsTestExecution() && this.ShouldAddFCCRunSettings())
            {
                return this.userRunSettingsService.AddFCCRunSettings(inputRunSettingDocument, configurationInfo, this.userRunSettingsProjectDetailsLookup, this.fccMsTestAdapterPath);
            }

            return null;
        }

        private bool ShouldAddFCCRunSettings()
        {
            return this.IsCollecting && this.userRunSettingsProjectDetailsLookup?.Count > 0;
        }

        #endregion

        public async Task CollectAsync(IOperation operation, ITestOperation testOperation)
        {
            await this.CleanUpAsync(testOperation);

            string[] coberturaFiles = this.GetCoberturaFiles(operation);
            if (coberturaFiles.Length == 0)
            {
                await this.logger.LogAsync("No cobertura files for ms code coverage.");
            }

            this.fccEngine.RunAndProcessReport(coberturaFiles, this.coverageProjectsByType.All);
        }

        private string[] GetCoberturaFiles(IOperation operation)
        {
            IEnumerable<Uri> resultsUris = operation.GetRunSettingsMsDataCollectorResultUri();
            string[] coberturaFiles = new string[0];
            if (resultsUris != null)
            {
                coberturaFiles = resultsUris.Select(uri => uri.LocalPath).Where(f => f.EndsWith(".cobertura.xml")).ToArray();
            }

            return coberturaFiles;
        }

        public void StopCoverage()
        {
            this.fccEngine.StopCoverage();
        }



        public Task TestExecutionNotFinishedAsync(ITestOperation testOperation)
        {
            return this.CleanUpAsync(testOperation);
        }

        private async Task CleanUpAsync(ITestOperation testOperation)
        {
            this.coverageProjectsByType = await CoverageProjectsByType.CreateAsync(testOperation);
            await this.templatedRunSettingsService.CleanUpAsync(this.coverageProjectsByType.RunSettings);
            this.collectionStatus = MsCodeCoverageCollectionStatus.NotCollecting;
        }
    }
}
