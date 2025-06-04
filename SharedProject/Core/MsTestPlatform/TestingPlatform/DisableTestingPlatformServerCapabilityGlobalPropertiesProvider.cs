using System;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FineCodeCoverage.Core.MsTestPlatform.TestingPlatform;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Options;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Build;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace FineCodeCoverage.Engine.MsTestPlatform.TestingPlatform
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
    internal class DisableTestingPlatformServerCapabilityGlobalPropertiesProvider : StaticGlobalPropertiesProviderBase
    {
        private readonly UnconfiguredProject _unconfiguredProject;
        private readonly IOptionsProvider<RunOptions> _runOptionsProvider;
        private readonly IOptionsProvider<OutputOptions> _outputOptionsProvider;
        private readonly ICoverageProjectSettingsManager _coverageProjectSettingsManager;

        [ImportingConstructor]
        public DisableTestingPlatformServerCapabilityGlobalPropertiesProvider(
            IProjectService projectService,
            UnconfiguredProject unconfiguredProject,
            IOptionsProvider<RunOptions> runOptionsProvider,
            IOptionsProvider<OutputOptions> outputOptionsProvider,
            ICoverageProjectSettingsManager coverageProjectSettingsManager
        )
          : base(projectService.Services)
        {
            this._unconfiguredProject = unconfiguredProject;
            this._runOptionsProvider = runOptionsProvider;
            this._outputOptionsProvider = outputOptionsProvider;
            this._coverageProjectSettingsManager = coverageProjectSettingsManager;
        }

        private bool AllProjectsDisabled()
        {
            RunOptions appOptions = this._runOptionsProvider.Get();
            return !appOptions.Enabled && appOptions.DisabledNoCoverage;
        }

        private static async Task<bool> IsTestProjectAsync(ConfiguredProject configuredProject)
        {
            Microsoft.VisualStudio.ProjectSystem.Properties.IProjectProperties commonProperties = configuredProject.Services.ProjectPropertiesProvider.GetCommonProperties();
            string isTestProjectPropertValue = await commonProperties.GetEvaluatedPropertyValueAsync("IsTestProject");
            return !string.IsNullOrEmpty(isTestProjectPropertValue) &&
                bool.TryParse(isTestProjectPropertValue, out bool isTestProject) && isTestProject;
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
                ConfiguredProject configuredProject = await this._unconfiguredProject.GetSuggestedConfiguredProjectAsync();
                return await IsTestProjectAsync(configuredProject) && await NoTUnitPackageReferenceAsync(configuredProject);
            }
            catch { }

            return false;
        }

        private async Task<bool> ProjectEnabledAsync()
        {
            Guid? projectGuid = await this.GetProjectGuidAsync();
            if (!projectGuid.HasValue) return false;

            CoverageProject coverageProject = this.GetCoverageProject(projectGuid.Value);
            ICoverageSettings projectSettings = await this._coverageProjectSettingsManager.GetSettingsAsync(coverageProject);
            return projectSettings.Enabled;
        }

        private async Task<Guid?> GetProjectGuidAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            object hostObject = this._unconfiguredProject.Services.HostObject;

            var vsHierarchy = (IVsHierarchy)hostObject;
            if (vsHierarchy == null)
            {
                return null;
            }

            bool success = vsHierarchy.GetGuidProperty((uint)VSConstants.VSITEMID.Root, (int)__VSHPROPID.VSHPROPID_ProjectIDGuid, out Guid projectGuid) == VSConstants.S_OK;

            return success ? projectGuid : (Guid?)null;
        }

        private CoverageProject GetCoverageProject(Guid projectGuid)
            => new CoverageProject(this._outputOptionsProvider, null, this._coverageProjectSettingsManager, null)
            {
                Id = projectGuid,
                ProjectFilePath = this._unconfiguredProject.FullPath
            };

        public override async Task<IImmutableDictionary<string, string>> GetGlobalPropertiesAsync(CancellationToken cancellationToken)
            => await this.IsApplicableAsync() && !this.AllProjectsDisabled() && await this.ProjectEnabledAsync()
                ? Empty.PropertiesMap.Add("DisableTestingPlatformServerCapability", "true")
                : (IImmutableDictionary<string, string>)Empty.PropertiesMap;
    }
}
