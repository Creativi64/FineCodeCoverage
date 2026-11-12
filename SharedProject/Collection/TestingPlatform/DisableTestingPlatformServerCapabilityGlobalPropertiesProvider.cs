using System;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Collection.CoverageProjectManagement;
using FineCodeCoverage.Collection.CoverageProjectManagement.Settings;
using FineCodeCoverage.Collection.TestingPlatform.TUnit;
using FineCodeCoverage.Options.Base;
using FineCodeCoverage.Options.Output;
using FineCodeCoverage.Options.Run;
using FineCodeCoverage.Utilities.Wrappers;
using FineCodeCoverage.VSAbstractions.OutputWindow;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Build;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace FineCodeCoverage.Collection.TestingPlatform
{
    // https://github.com/dotnet/project-system
    // https://github.com/microsoft/VSProjectSystem/blob/master/doc/extensibility/IProjectGlobalPropertiesProvider.md
    [Export(typeof(IProjectGlobalPropertiesProvider))]

    // https://github.com/microsoft/testfx/blob/main/src/Platform/Microsoft.Testing.Platform/buildMultiTargeting/Microsoft.Testing.Platform.props
    /*
            https://github.com/microsoft/VSProjectSystem/blob/master/doc/overview/about_project_capabilities.md
            Classes exported via MEF can declare the project capabilities under which they apply.

            See https://learn.microsoft.com/en-gb/dotnet/api/microsoft.visualstudio.shell.interop.vsprojectcapabilityexpressionmatcher?view=visualstudiosdk-2022
            For expression syntax
    */
    [AppliesTo("TestContainer")]
    internal sealed class DisableTestingPlatformServerCapabilityGlobalPropertiesProvider : StaticGlobalPropertiesProviderBase
    {
        private readonly UnconfiguredProject _unconfiguredProject;
        private readonly IOptionsProvider<RunOptions> _runOptionsProvider;
        private readonly IOptionsProvider<OutputOptions> _outputOptionsProvider;
        private readonly ICoverageProjectSettingsManager _coverageProjectSettingsManager;
        private readonly IFileUtil _fileUtil;
        private readonly ILogger _logger;

        [ImportingConstructor]
        public DisableTestingPlatformServerCapabilityGlobalPropertiesProvider(
            IProjectService projectService,
            UnconfiguredProject unconfiguredProject,
            IOptionsProvider<RunOptions> runOptionsProvider,
            IOptionsProvider<OutputOptions> outputOptionsProvider,
            ICoverageProjectSettingsManager coverageProjectSettingsManager,
            IFileUtil fileUtil,
            ILogger logger)
          : base(projectService.Services)
        {
            _unconfiguredProject = unconfiguredProject;
            _runOptionsProvider = runOptionsProvider;
            _outputOptionsProvider = outputOptionsProvider;
            _coverageProjectSettingsManager = coverageProjectSettingsManager;
            _fileUtil = fileUtil;
            _logger = logger;
        }

        private bool AllProjectsDisabled()
        {
            RunOptions appOptions = _runOptionsProvider.Get();
            return !appOptions.Enabled && appOptions.DisabledNoCoverage;
        }

        private async Task<bool> ForcedToVsTestAsync()
        {
            RunOptions options = _runOptionsProvider.Get();
            if (options.DisableTestingPlatformServer)
            {
                return true;
            }

            if (!options.CollectTestingPlatformCoverageAfterTestRun)
            {
                return false;
            }

            try
            {
                ConfiguredProject configuredProject = await _unconfiguredProject.GetSuggestedConfiguredProjectAsync();
                Microsoft.VisualStudio.ProjectSystem.Properties.IProjectProperties commonProperties = configuredProject.Services.ProjectPropertiesProvider.GetCommonProperties();
                string assetsPath = await commonProperties.GetEvaluatedPropertyValueAsync("ProjectAssetsFile");
                if (string.IsNullOrWhiteSpace(assetsPath))
                {
                    string extensionsPath = await commonProperties.GetEvaluatedPropertyValueAsync("MSBuildProjectExtensionsPath");
                    if (string.IsNullOrWhiteSpace(extensionsPath))
                    {
                        return false;
                    }

                    assetsPath = Path.Combine(extensionsPath, "project.assets.json");
                }

                if (!Path.IsPathRooted(assetsPath))
                {
                    assetsPath = Path.Combine(Path.GetDirectoryName(_unconfiguredProject.FullPath), assetsPath);
                }

                return _fileUtil.Exists(assetsPath) &&
                    TestingPlatformCompatibility.RequiresVsTest(_fileUtil.ReadAllText(assetsPath));
            }
            catch (Exception exception)
            {
                await _logger.LogAsync($"{_unconfiguredProject.FullPath} - unable to determine automatic VSTest compatibility from restored packages.", exception.Message);
                return false;
            }
        }

        private static async Task<bool> IsTestProjectAsync(ConfiguredProject configuredProject)
        {
            Microsoft.VisualStudio.ProjectSystem.Properties.IProjectProperties commonProperties = configuredProject.Services.ProjectPropertiesProvider.GetCommonProperties();
            string isTestProjectPropertyValue = await commonProperties.GetEvaluatedPropertyValueAsync("IsTestProject");
            return !string.IsNullOrEmpty(isTestProjectPropertyValue) &&
                bool.TryParse(isTestProjectPropertyValue, out bool isTestProject) && isTestProject;
        }

        private static async Task<bool> NoTUnitPackageReferenceAsync(ConfiguredProject configuredProject)
        {
            // although have ITUnitInstalledPackagesService it is not ready the first time this is called.
            IImmutableSet<Microsoft.VisualStudio.ProjectSystem.References.IUnresolvedPackageReference> references = await configuredProject.Services.PackageReferences.GetUnresolvedReferencesAsync();
            return !references.Any(r => r.UnevaluatedInclude == TUnitConstants.TUnitPackageId);
        }

        private async Task<bool> IsApplicableAsync()
        {
            try
            {
                ConfiguredProject configuredProject = await _unconfiguredProject.GetSuggestedConfiguredProjectAsync();
                return await IsTestProjectAsync(configuredProject) && await NoTUnitPackageReferenceAsync(configuredProject);
            }
            catch
            {
            }

            return false;
        }

        private async Task<bool> ProjectEnabledAsync()
        {
            Guid? projectGuid = await GetProjectGuidAsync();
            if (!projectGuid.HasValue)
            {
                return false;
            }

            CoverageProject coverageProject = GetCoverageProject(projectGuid.Value);
            ICoverageSettings projectSettings = await _coverageProjectSettingsManager.GetSettingsAsync(coverageProject);
            return projectSettings.Enabled;
        }

        private async Task<Guid?> GetProjectGuidAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            object hostObject = _unconfiguredProject.Services.HostObject;

            var vsHierarchy = (IVsHierarchy)hostObject;
            if (vsHierarchy == null)
            {
                return null;
            }

            bool success = vsHierarchy.GetGuidProperty((uint)VSConstants.VSITEMID.Root, (int)__VSHPROPID.VSHPROPID_ProjectIDGuid, out Guid projectGuid) == VSConstants.S_OK;

            return success ? projectGuid : null;
        }

        private CoverageProject GetCoverageProject(Guid projectGuid)
            => new(_outputOptionsProvider, null, _coverageProjectSettingsManager, null)
            {
                Id = projectGuid,
                ProjectFilePath = _unconfiguredProject.FullPath,
            };

        public override async Task<IImmutableDictionary<string, string>> GetGlobalPropertiesAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (AllProjectsDisabled() || !await ForcedToVsTestAsync() || !await IsApplicableAsync() || !await ProjectEnabledAsync())
            {
                return Empty.PropertiesMap;
            }

            await _logger.LogAsync($"{_unconfiguredProject.FullPath} - selecting VSTest compatibility for automatic coverage. DisableTestingPlatformServerCapability=true; Test Explorer selections are preserved.");
            return Empty.PropertiesMap.Add("DisableTestingPlatformServerCapability", "true");
        }
    }
}
