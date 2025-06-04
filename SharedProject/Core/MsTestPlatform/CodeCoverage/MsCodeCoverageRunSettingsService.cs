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

            public bool HasTemplated() => Templated.Count > 0;

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

        private const string ZipPrefix = "microsoft.codecoverage";
        private const string ZipDirectoryName = "msCodeCoverage";
        private const string MsCodeCoverageMessage = "Ms code coverage";
        private readonly IToolUnzipper _toolUnzipper;
        private readonly IOptionsProvider<RunOptions> _runOptionsProvider;
        private readonly ICoverageToolOutputManager _coverageOutputManager;
        private readonly IShimCopier _shimCopier;
        private readonly ILogger _logger;
        private readonly IFCCEngine _fccEngine;
        private readonly IUserRunSettingsService _userRunSettingsService;
        private readonly ITemplatedRunSettingsService _templatedRunSettingsService;
        internal Dictionary<string, IUserRunSettingsProjectDetails> _userRunSettingsProjectDetailsLookup; // for tests
        private string _fccMsTestAdapterPath;
        private string _shimPath;
        private CoverageProjectsByType _coverageProjectsByType;
        private bool _useMsCodeCoverage;
        internal MsCodeCoverageCollectionStatus _collectionStatus; // for tests
        private RunMsCodeCoverage _runMsCodeCoverage;

        private bool IsCollecting => _collectionStatus == MsCodeCoverageCollectionStatus.Collecting;

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
            _toolUnzipper = toolUnzipper;
            _runOptionsProvider = runOptionsProvider;
            _coverageOutputManager = coverageOutputManager;
            _shimCopier = shimCopier;
            _logger = logger;
            _userRunSettingsService = userRunSettingsService;
            _templatedRunSettingsService = templatedRunSettingsService;
            _fccEngine = fccEngine;
        }

        public Task InitializeAsync(string appDataFolderPath, CancellationToken cancellationToken)
        {
            string zipDestination = _toolUnzipper.EnsureUnzipped(appDataFolderPath, ZipDirectoryName, ZipPrefix, cancellationToken);
            _fccMsTestAdapterPath = Path.Combine(zipDestination, "build", "netstandard2.0");
            _shimPath = Path.Combine(zipDestination, "build", "netstandard2.0", "CodeCoverage", "coreclr", "Microsoft.VisualStudio.CodeCoverage.Shim.dll");
            return Task.CompletedTask;
        }

        #region set up for collection

        public async Task<MsCodeCoverageCollectionStatus> IsCollectingAsync(ITestOperation testOperation)
        {
            await InitializeIsCollectingAsync(testOperation);
            if (_runMsCodeCoverage != RunMsCodeCoverage.No)
            {
                await TrySetUpForCollectionAsync(testOperation.SolutionDirectory);
            }

            return _collectionStatus;
        }

        private async Task TrySetUpForCollectionAsync(string solutionDirectory)
        {
            IUserRunSettingsAnalysisResult analysisResult = await TryAnalyseUserRunSettingsAsync();
            if (!analysisResult.Ok())
            {
                return;
            }

            await SetUpForCollectionAsync(
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
            await PrepareCoverageProjectsAsync();
            SetUserRunSettingsProjectDetails();

            await GenerateTemplatedRunSettingsIfRequiredAsync(
                specifiedMsCodeCoverageInRunSettings,
                coverageProjectsForShim,
                solutionDirectory
            );
            CopyShimWhenCollecting(coverageProjectsForShim);
        }

        private Task InitializeIsCollectingAsync(ITestOperation testOperation)
        {
            _runMsCodeCoverage = _runOptionsProvider.Get().RunMsCodeCoverage;
            _useMsCodeCoverage = _runMsCodeCoverage == RunMsCodeCoverage.Yes;
            _userRunSettingsProjectDetailsLookup = null;
            return CleanUpAsync(testOperation);
        }

        private async Task<IUserRunSettingsAnalysisResult> TryAnalyseUserRunSettingsAsync()
        {
            IUserRunSettingsAnalysisResult analysisResult = null;
            try
            {
                analysisResult = await AnalyseUserRunSettingsAsync();
            }
            catch (Exception exc)
            {
                await ExceptionAnalysingUserRunSettingsAsync(exc);
            }

            return analysisResult;
        }

        private Task ExceptionAnalysingUserRunSettingsAsync(Exception exc)
        {
            _collectionStatus = MsCodeCoverageCollectionStatus.Error;
            return _logger.LogAsync("Exception analysing runsettings files", exc.ToString());
        }

        private async Task<IUserRunSettingsAnalysisResult> AnalyseUserRunSettingsAsync()
        {
            IUserRunSettingsAnalysisResult analysisResult = _userRunSettingsService.Analyse(
                    _coverageProjectsByType.RunSettings,
                    _useMsCodeCoverage,
                    _fccMsTestAdapterPath
                );

            if (analysisResult.Suitable)
            {
                await CollectingIfUserRunSettingsOnlyAsync();
            }

            return analysisResult;
        }

        private async Task GenerateTemplatedRunSettingsIfRequiredAsync(
            bool runSettingsSpecifiedMsCodeCoverage,
            List<ICoverageProject> coverageProjectsForShim,
            string solutionDirectory
        )
        {
            if (!ShouldGenerateTemplatedRunSettings(runSettingsSpecifiedMsCodeCoverage))
            {
                return;
            }

            await GenerateTemplatedRunSettingsAsync(coverageProjectsForShim, solutionDirectory);
        }
        private async Task GenerateTemplatedRunSettingsAsync(
            List<ICoverageProject> coverageProjectsForShim,
            string solutionDirectory
        )
        {
            IProjectRunSettingsFromTemplateResult generationResult = await _templatedRunSettingsService.GenerateAsync(
                _coverageProjectsByType.Templated,
                solutionDirectory,
                _fccMsTestAdapterPath
            );

            await ProcessTemplateGenerationResultAsync(generationResult, coverageProjectsForShim);
        }

        private bool ShouldGenerateTemplatedRunSettings(bool runSettingsSpecifiedMsCodeCoverage)
            => _coverageProjectsByType.HasTemplated() &&
            (_useMsCodeCoverage || runSettingsSpecifiedMsCodeCoverage);

        private async Task ProcessTemplateGenerationResultAsync(IProjectRunSettingsFromTemplateResult generationResult, List<ICoverageProject> coverageProjectsForShim)
        {
            if (generationResult.ExceptionReason == null)
            {
                await CollectingWithTemplateAsync(generationResult, coverageProjectsForShim);
            }
            else
            {
                IExceptionReason exceptionReason = generationResult.ExceptionReason;
                await _logger.LogAsync(exceptionReason.Reason, exceptionReason.Exception.ToString());
                _collectionStatus = MsCodeCoverageCollectionStatus.Error;
            }
        }

        private async Task CollectingWithTemplateAsync(IProjectRunSettingsFromTemplateResult generationResult, List<ICoverageProject> coverageProjectsForShim)
        {
            coverageProjectsForShim.AddRange(generationResult.CoverageProjectsWithFCCMsTestAdapter);
            string leadingMessage = generationResult.CustomTemplatePaths.Count != 0 ? $"{MsCodeCoverageMessage} - custom template paths" : MsCodeCoverageMessage;
            IEnumerable<string> loggerMessages = new List<string> { leadingMessage }.Concat(generationResult.CustomTemplatePaths.Distinct());
            await _logger.LogAsync(loggerMessages);
            _collectionStatus = MsCodeCoverageCollectionStatus.Collecting;
        }

        private async Task CollectingIfUserRunSettingsOnlyAsync()
        {
            if (_coverageProjectsByType.HasTemplated())
            {
                return;
            }

            _collectionStatus = MsCodeCoverageCollectionStatus.Collecting;
            await _logger.LogAsync($"{MsCodeCoverageMessage} with user runsettings");
        }

        private void CopyShimWhenCollecting(List<ICoverageProject> coverageProjectsForShim)
        {
            if (!IsCollecting)
            {
                return;
            }

            _shimCopier.Copy(_shimPath, coverageProjectsForShim);
        }

        private async Task PrepareCoverageProjectsAsync()
        {
            await _coverageOutputManager.SetProjectCoverageOutputFolderAsync(_coverageProjectsByType.All);
            foreach (ICoverageProject coverageProject in _coverageProjectsByType.All)
            {
                _ = await coverageProject.PrepareForCoverageAsync(CancellationToken.None, false);
            }
        }

        private void SetUserRunSettingsProjectDetails()
        {
            _userRunSettingsProjectDetailsLookup = new Dictionary<string, IUserRunSettingsProjectDetails>();
            foreach (ICoverageProject coverageProjectWithRunSettings in _coverageProjectsByType.RunSettings)
            {
                var userRunSettingsProjectDetails = new UserRunSettingsProjectDetails
                {
                    Settings = coverageProjectWithRunSettings.Settings,
                    CoverageOutputFolder = coverageProjectWithRunSettings.CoverageOutputFolder,
                    TestDllFile = coverageProjectWithRunSettings.TestDllFile,
                    ExcludedReferencedProjects = coverageProjectWithRunSettings.ExcludedReferencedProjects,
                    IncludedReferencedProjects = coverageProjectWithRunSettings.IncludedReferencedProjects
                };
                _userRunSettingsProjectDetailsLookup.Add(coverageProjectWithRunSettings.TestDllFile, userRunSettingsProjectDetails);
            }
        }

        #endregion

        #region IRunSettingsService
        public IXPathNavigable AddRunSettings(
            IXPathNavigable inputRunSettingDocument,
            IRunSettingsConfigurationInfo configurationInfo,
            Microsoft.VisualStudio.TestWindow.Extensibility.ILogger log
        ) => configurationInfo.IsTestExecution() && ShouldAddFCCRunSettings()
                ? _userRunSettingsService.AddFCCRunSettings(
                    inputRunSettingDocument,
                    configurationInfo,
                    _userRunSettingsProjectDetailsLookup,
                    _fccMsTestAdapterPath)
                : null;

        private bool ShouldAddFCCRunSettings()
            => IsCollecting && _userRunSettingsProjectDetailsLookup?.Count > 0;

        #endregion

        public async Task CollectAsync(IOperation operation, ITestOperation testOperation)
        {
            await CleanUpAsync(testOperation);

            string[] coberturaFiles = GetCoberturaFiles(operation);
            if (coberturaFiles.Length == 0)
            {
                await _logger.LogAsync("No cobertura files for ms code coverage.");
            }

            _fccEngine.RunAndProcessReport(coberturaFiles, _coverageProjectsByType.All);
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

        public void StopCoverage() => _fccEngine.StopCoverage();

        public Task TestExecutionNotFinishedAsync(ITestOperation testOperation)
            => CleanUpAsync(testOperation);

        private async Task CleanUpAsync(ITestOperation testOperation)
        {
            _coverageProjectsByType = await CoverageProjectsByType.CreateAsync(testOperation);
            await _templatedRunSettingsService.CleanUpAsync(_coverageProjectsByType.RunSettings);
            _collectionStatus = MsCodeCoverageCollectionStatus.NotCollecting;
        }
    }
}
