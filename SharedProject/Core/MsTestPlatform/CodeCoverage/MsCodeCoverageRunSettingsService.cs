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

            public bool HasTemplated() => this.Templated.Count > 0;

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

        private readonly IToolUnzipper _toolUnzipper;
        private readonly IOptionsProvider<RunOptions> _runOptionsProvider;
        private readonly ICoverageToolOutputManager _coverageOutputManager;
        private readonly IShimCopier _shimCopier;
        private readonly ILogger _logger;
        private readonly IFCCEngine _fccEngine;

        private const string zipPrefix = "microsoft.codecoverage";
        private const string zipDirectoryName = "msCodeCoverage";

        private const string msCodeCoverageMessage = "Ms code coverage";
        internal Dictionary<string, IUserRunSettingsProjectDetails> userRunSettingsProjectDetailsLookup; // for tests

        private readonly IUserRunSettingsService _userRunSettingsService;
        private readonly ITemplatedRunSettingsService _templatedRunSettingsService;
        private string _fccMsTestAdapterPath;
        private string _shimPath;

        private CoverageProjectsByType _coverageProjectsByType;
        private bool _useMsCodeCoverage;

        internal MsCodeCoverageCollectionStatus collectionStatus; // for tests
        private RunMsCodeCoverage _runMsCodeCoverage;

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
            this._toolUnzipper = toolUnzipper;
            this._runOptionsProvider = runOptionsProvider;
            this._coverageOutputManager = coverageOutputManager;
            this._shimCopier = shimCopier;
            this._logger = logger;
            this._userRunSettingsService = userRunSettingsService;
            this._templatedRunSettingsService = templatedRunSettingsService;
            this._fccEngine = fccEngine;
        }

        public Task InitializeAsync(string appDataFolderPath, CancellationToken cancellationToken)
        {
            string zipDestination = this._toolUnzipper.EnsureUnzipped(appDataFolderPath, zipDirectoryName, zipPrefix, cancellationToken);
            this._fccMsTestAdapterPath = Path.Combine(zipDestination, "build", "netstandard2.0");
            this._shimPath = Path.Combine(zipDestination, "build", "netstandard2.0", "CodeCoverage", "coreclr", "Microsoft.VisualStudio.CodeCoverage.Shim.dll");
            return Task.CompletedTask;
        }

        #region set up for collection

        public async Task<MsCodeCoverageCollectionStatus> IsCollectingAsync(ITestOperation testOperation)
        {
            await this.InitializeIsCollectingAsync(testOperation);
            if (this._runMsCodeCoverage != RunMsCodeCoverage.No)
            {
                await this.TrySetUpForCollectionAsync(testOperation.SolutionDirectory);
            }

            return this.collectionStatus;
        }

        private async Task TrySetUpForCollectionAsync(string solutionDirectory)
        {
            IUserRunSettingsAnalysisResult analysisResult = await this.TryAnalyseUserRunSettingsAsync();
            if (!analysisResult.Ok())
            {
                return;
            }

            await this.SetUpForCollectionAsync(
                analysisResult.ProjectsWithFCCMsTestAdapter,
                analysisResult.SpecifiedMsCodeCoverage,
                solutionDirectory
            );
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
            this._runMsCodeCoverage = this._runOptionsProvider.Get().RunMsCodeCoverage;
            this._useMsCodeCoverage = this._runMsCodeCoverage == RunMsCodeCoverage.Yes;
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
            return this._logger.LogAsync("Exception analysing runsettings files", exc.ToString());
        }

        private async Task<IUserRunSettingsAnalysisResult> AnalyseUserRunSettingsAsync()
        {
            IUserRunSettingsAnalysisResult analysisResult = this._userRunSettingsService.Analyse(
                    this._coverageProjectsByType.RunSettings,
                    this._useMsCodeCoverage,
                    this._fccMsTestAdapterPath
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
            if (!this.ShouldGenerateTemplatedRunSettings(runSettingsSpecifiedMsCodeCoverage))
            {
                return;
            }

            await this.GenerateTemplatedRunSettingsAsync(coverageProjectsForShim, solutionDirectory);
        }
        private async Task GenerateTemplatedRunSettingsAsync(
            List<ICoverageProject> coverageProjectsForShim,
            string solutionDirectory
        )
        {
            IProjectRunSettingsFromTemplateResult generationResult = await this._templatedRunSettingsService.GenerateAsync(
                this._coverageProjectsByType.Templated,
                solutionDirectory,
                this._fccMsTestAdapterPath
            );

            await this.ProcessTemplateGenerationResultAsync(generationResult, coverageProjectsForShim);
        }

        private bool ShouldGenerateTemplatedRunSettings(bool runSettingsSpecifiedMsCodeCoverage)
            => this._coverageProjectsByType.HasTemplated() &&
            (this._useMsCodeCoverage || runSettingsSpecifiedMsCodeCoverage);

        private async Task ProcessTemplateGenerationResultAsync(IProjectRunSettingsFromTemplateResult generationResult, List<ICoverageProject> coverageProjectsForShim)
        {
            if (generationResult.ExceptionReason == null)
            {
                await this.CollectingWithTemplateAsync(generationResult, coverageProjectsForShim);
            }
            else
            {
                IExceptionReason exceptionReason = generationResult.ExceptionReason;
                await this._logger.LogAsync(exceptionReason.Reason, exceptionReason.Exception.ToString());
                this.collectionStatus = MsCodeCoverageCollectionStatus.Error;
            }
        }

        private async Task CollectingWithTemplateAsync(IProjectRunSettingsFromTemplateResult generationResult, List<ICoverageProject> coverageProjectsForShim)
        {
            coverageProjectsForShim.AddRange(generationResult.CoverageProjectsWithFCCMsTestAdapter);
            string leadingMessage = generationResult.CustomTemplatePaths.Count != 0 ? $"{msCodeCoverageMessage} - custom template paths" : msCodeCoverageMessage;
            IEnumerable<string> loggerMessages = new List<string> { leadingMessage }.Concat(generationResult.CustomTemplatePaths.Distinct());
            await this._logger.LogAsync(loggerMessages);
            this.collectionStatus = MsCodeCoverageCollectionStatus.Collecting;
        }

        private async Task CollectingIfUserRunSettingsOnlyAsync()
        {
            if (this._coverageProjectsByType.HasTemplated())
            {
                return;
            }

            this.collectionStatus = MsCodeCoverageCollectionStatus.Collecting;
            await this._logger.LogAsync($"{msCodeCoverageMessage} with user runsettings");
        }

        private void CopyShimWhenCollecting(List<ICoverageProject> coverageProjectsForShim)
        {
            if (!this.IsCollecting)
            {
                return;
            }

            this._shimCopier.Copy(this._shimPath, coverageProjectsForShim);
        }

        private async Task PrepareCoverageProjectsAsync()
        {
            await this._coverageOutputManager.SetProjectCoverageOutputFolderAsync(this._coverageProjectsByType.All);
            foreach (ICoverageProject coverageProject in this._coverageProjectsByType.All)
            {
                _ = await coverageProject.PrepareForCoverageAsync(CancellationToken.None, false);
            }
        }

        private void SetUserRunSettingsProjectDetails()
        {
            this.userRunSettingsProjectDetailsLookup = new Dictionary<string, IUserRunSettingsProjectDetails>();
            foreach (ICoverageProject coverageProjectWithRunSettings in this._coverageProjectsByType.RunSettings)
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
        public IXPathNavigable AddRunSettings(
            IXPathNavigable inputRunSettingDocument,
            IRunSettingsConfigurationInfo configurationInfo,
            Microsoft.VisualStudio.TestWindow.Extensibility.ILogger log
        ) => configurationInfo.IsTestExecution() && this.ShouldAddFCCRunSettings()
                ? this._userRunSettingsService.AddFCCRunSettings(
                    inputRunSettingDocument,
                    configurationInfo,
                    this.userRunSettingsProjectDetailsLookup,
                    this._fccMsTestAdapterPath)
                : null;

        private bool ShouldAddFCCRunSettings()
            => this.IsCollecting && this.userRunSettingsProjectDetailsLookup?.Count > 0;

        #endregion

        public async Task CollectAsync(IOperation operation, ITestOperation testOperation)
        {
            await this.CleanUpAsync(testOperation);

            string[] coberturaFiles = GetCoberturaFiles(operation);
            if (coberturaFiles.Length == 0)
            {
                await this._logger.LogAsync("No cobertura files for ms code coverage.");
            }

            this._fccEngine.RunAndProcessReport(coberturaFiles, this._coverageProjectsByType.All);
        }

        private static string[] GetCoberturaFiles(IOperation operation)
        {
            IEnumerable<Uri> resultsUris = operation.GetRunSettingsMsDataCollectorResultUri();
            string[] coberturaFiles = Array.Empty<string>();
            if (resultsUris != null)
            {
                coberturaFiles = resultsUris.Select(uri => uri.LocalPath).Where(f => f.EndsWith(".cobertura.xml")).ToArray();
            }

            return coberturaFiles;
        }

        public void StopCoverage() => this._fccEngine.StopCoverage();

        public Task TestExecutionNotFinishedAsync(ITestOperation testOperation)
            => this.CleanUpAsync(testOperation);

        private async Task CleanUpAsync(ITestOperation testOperation)
        {
            this._coverageProjectsByType = await CoverageProjectsByType.CreateAsync(testOperation);
            await this._templatedRunSettingsService.CleanUpAsync(this._coverageProjectsByType.RunSettings);
            this.collectionStatus = MsCodeCoverageCollectionStatus.NotCollecting;
        }
    }
}