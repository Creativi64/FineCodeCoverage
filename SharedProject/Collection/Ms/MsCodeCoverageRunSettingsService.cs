using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.XPath;
using FineCodeCoverage.Collection.CoverageProjectManagement;
using FineCodeCoverage.Collection.CoverageProjectManagement.ReferencedProjects;
using FineCodeCoverage.Collection.CoverageProjectManagement.Settings;
using FineCodeCoverage.Collection.CoverageToolOutput;
using FineCodeCoverage.Collection.Engine;
using FineCodeCoverage.Collection.TestExplorer;
using FineCodeCoverage.Initialization;
using FineCodeCoverage.Initialization.ToolZip;
using FineCodeCoverage.Options.Base;
using FineCodeCoverage.Options.Run;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using ILogger = FineCodeCoverage.VSAbstractions.OutputWindow.ILogger;
using Task = System.Threading.Tasks.Task;

namespace FineCodeCoverage.Collection.Ms
{
    [Export(typeof(IMsCodeCoverageRunSettingsService))]
    [Export(typeof(IRunSettingsService))]
    [Export(typeof(IAppDataFolderPathDependent))]
    internal sealed class MsCodeCoverageRunSettingsService : IMsCodeCoverageRunSettingsService, IRunSettingsService, IAppDataFolderPathDependent
    {
        public string Name => "Fine Code Coverage MsCodeCoverageRunSettingsService";

        private sealed class UserRunSettingsProjectDetails : IUserRunSettingsProjectDetails
        {
            public ICoverageSettings Settings { get; set; }

            public string CoverageOutputFolder { get; set; }

            public string TestDllFile { get; set; }

            public List<IReferencedProject> ExcludedReferencedProjects { get; set; }

            public List<IReferencedProject> IncludedReferencedProjects { get; set; }
        }

        private sealed class CoverageProjectsByType
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
                    Templated = coverageProjectsWithoutRunSettings,
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
        private string _fccMsTestAdapterPath;
        private string _shimPath;
        private CoverageProjectsByType _coverageProjectsByType;
        private bool _useMsCodeCoverage;

        private bool IsCollecting => CollectionStatus == MsCodeCoverageCollectionStatus.Collecting;

        // internal properties for tests
        internal Dictionary<string, IUserRunSettingsProjectDetails> UserRunSettingsProjectDetailsLookup { get; set; }

        internal MsCodeCoverageCollectionStatus CollectionStatus { get; set; }

        [ImportingConstructor]
        public MsCodeCoverageRunSettingsService(
            IToolUnzipper toolUnzipper,
            IOptionsProvider<RunOptions> runOptionsProvider,
            ICoverageToolOutputManager coverageOutputManager,
            IUserRunSettingsService userRunSettingsService,
            ITemplatedRunSettingsService templatedRunSettingsService,
            IShimCopier shimCopier,
            ILogger logger,
            IFCCEngine fccEngine)
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
            await TrySetUpForCollectionAsync();

            return CollectionStatus;
        }

        private async Task TrySetUpForCollectionAsync()
        {
            IUserRunSettingsAnalysisResult analysisResult = await TryAnalyseUserRunSettingsAsync();
            if (!analysisResult.Ok())
            {
                return;
            }

            await SetUpForCollectionAsync();
        }

        private async Task SetUpForCollectionAsync()
        {
            await PrepareCoverageProjectsAsync();
            SetUserRunSettingsProjectDetails();

            // FCC injects the Code Coverage data collector and its test adapter path in-memory through
            // IRunSettingsService.AddRunSettings (see UserRunSettingsService.AddFCCRunSettings) for every
            // collected project - both those with their own runsettings and those without.  Nothing is
            // written to the project files, which avoids the project-reload race that previously dropped
            // coverage on alternating runs.
            CollectionStatus = MsCodeCoverageCollectionStatus.Collecting;
            await _logger.LogAsync(MsCodeCoverageMessage);

            // The shim is required wherever the FCC ms test adapter is used i.e. every collected project.
            CopyShimWhenCollecting(_coverageProjectsByType.All);
        }

        private Task InitializeIsCollectingAsync(ITestOperation testOperation)
        {
            // Ms code coverage is always-on for classic VSTest projects (the Coverlet/OpenCover fallback was removed).
            _useMsCodeCoverage = true;
            UserRunSettingsProjectDetailsLookup = null;
            return CleanUpAsync(testOperation);
        }

        private async Task<IUserRunSettingsAnalysisResult> TryAnalyseUserRunSettingsAsync()
        {
            try
            {
                return _userRunSettingsService.Analyse(
                    _coverageProjectsByType.RunSettings,
                    _useMsCodeCoverage,
                    _fccMsTestAdapterPath);
            }
            catch (Exception exc)
            {
                await ExceptionAnalysingUserRunSettingsAsync(exc);
                return null;
            }
        }

        private Task ExceptionAnalysingUserRunSettingsAsync(Exception exc)
        {
            CollectionStatus = MsCodeCoverageCollectionStatus.Error;
            return _logger.LogAsync("Exception analysing runsettings files", exc.ToString());
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
            // Every collected project (with or without its own runsettings) is keyed by its test dll so
            // that AddFCCRunSettings can inject coverage for whichever containers are in a given run.
            UserRunSettingsProjectDetailsLookup = new Dictionary<string, IUserRunSettingsProjectDetails>();
            foreach (ICoverageProject coverageProject in _coverageProjectsByType.All)
            {
                // A project without a built test assembly cannot be collected (and is the dictionary key),
                // so skip it rather than failing the whole collection set up.
                if (string.IsNullOrEmpty(coverageProject.TestDllFile))
                {
                    continue;
                }

                UserRunSettingsProjectDetailsLookup[coverageProject.TestDllFile] = new UserRunSettingsProjectDetails
                {
                    Settings = coverageProject.Settings,
                    CoverageOutputFolder = coverageProject.CoverageOutputFolder,
                    TestDllFile = coverageProject.TestDllFile,
                    ExcludedReferencedProjects = coverageProject.ExcludedReferencedProjects,
                    IncludedReferencedProjects = coverageProject.IncludedReferencedProjects,
                };
            }
        }

        #endregion

        #region IRunSettingsService
        public IXPathNavigable AddRunSettings(
            IXPathNavigable inputRunSettingDocument,
            IRunSettingsConfigurationInfo configurationInfo,
            Microsoft.VisualStudio.TestWindow.Extensibility.ILogger log) => configurationInfo.IsTestExecution() && ShouldAddFCCRunSettings()
                ? _userRunSettingsService.AddFCCRunSettings(
                    inputRunSettingDocument,
                    configurationInfo,
                    UserRunSettingsProjectDetailsLookup,
                    _fccMsTestAdapterPath)
                : null;

        private bool ShouldAddFCCRunSettings()
            => IsCollecting && UserRunSettingsProjectDetailsLookup?.Count > 0;

        #endregion

        public async Task CollectAsync(IOperation operation, ITestOperation testOperation)
        {
            await CleanUpAsync(testOperation);

            Uri[] resultUris = operation.GetRunSettingsMsDataCollectorResultUri()?.ToArray() ?? Array.Empty<Uri>();
            string[] coberturaFiles = GetCoberturaFiles(resultUris);
            if (coberturaFiles.Length == 0)
            {
                await LogNoCoberturaFilesAsync(resultUris);
            }

            _fccEngine.RunAndProcessReport(coberturaFiles, _coverageProjectsByType.All);
        }

        private static string[] GetCoberturaFiles(Uri[] resultUris)
            => resultUris.Select(uri => uri.LocalPath).Where(f => f.EndsWith(".cobertura.xml")).ToArray();

        // Logs the raw result attachments the test platform reported for the Code Coverage
        // data collector so an empty/non-cobertura result can be diagnosed:
        //   - zero attachments => the collector produced no output (e.g. a leftover testhost /
        //     CodeCoverage.exe process from a previous run, or the collector failed to attach);
        //   - attachments present but none ending in .cobertura.xml => the collector emitted a
        //     different format (e.g. a binary .coverage file) so the Cobertura format was not honoured.
        private Task LogNoCoberturaFilesAsync(Uri[] resultUris)
        {
            var messages = new List<string> { "No cobertura files for ms code coverage." };
            if (resultUris.Length == 0)
            {
                messages.Add("The Code Coverage data collector returned no result attachments for this run - the coverage tool produced no output. This is often a leftover testhost/CodeCoverage.exe process from a previous run or the collector failing to attach; restarting Visual Studio usually clears it.");
            }
            else
            {
                messages.Add($"The Code Coverage data collector returned {resultUris.Length} attachment(s) but none ended in .cobertura.xml (the Cobertura format may not have been honoured):");
                messages.AddRange(resultUris.Select(uri => uri.LocalPath));
            }

            return _logger.LogAsync(messages);
        }

        public void StopCoverage() => _fccEngine.StopCoverage();

        public Task TestExecutionNotFinishedAsync(ITestOperation testOperation)
            => CleanUpAsync(testOperation);

        private async Task CleanUpAsync(ITestOperation testOperation)
        {
            _coverageProjectsByType = await CoverageProjectsByType.CreateAsync(testOperation);
            await _templatedRunSettingsService.CleanUpAsync(_coverageProjectsByType.RunSettings);
            CollectionStatus = MsCodeCoverageCollectionStatus.NotCollecting;
        }
    }
}
