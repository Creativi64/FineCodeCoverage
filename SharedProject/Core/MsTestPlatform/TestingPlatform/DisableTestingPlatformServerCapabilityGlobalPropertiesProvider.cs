using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Options;
using Microsoft.VisualStudio.ProjectSystem.Build;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Immutable;
using System.Threading;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using System.Threading.Tasks;

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
    [AppliesTo("TestingPlatformServer.ExitOnProcessExitCapability | TestingPlatformServer.UseListTestsOptionForDiscoveryCapability")]
    internal class DisableTestingPlatformServerCapabilityGlobalPropertiesProvider : StaticGlobalPropertiesProviderBase
    {
        private readonly IUseTestingPlatformProtocolFeatureService useTestingPlatformProtocolFeatureService;
        private readonly UnconfiguredProject unconfiguredProject;
        private readonly IAppOptionsProvider appOptionsProvider;
        private readonly ICoverageProjectSettingsManager coverageProjectSettingsManager;

        [ImportingConstructor]
        public DisableTestingPlatformServerCapabilityGlobalPropertiesProvider(
            IUseTestingPlatformProtocolFeatureService useTestingPlatformProtocolFeatureService,
            IProjectService projectService,
            UnconfiguredProject unconfiguredProject,
            IAppOptionsProvider appOptionsProvider,
            ICoverageProjectSettingsManager coverageProjectSettingsManager
        )
          : base((IProjectCommonServices)projectService.Services)
        {
            this.useTestingPlatformProtocolFeatureService = useTestingPlatformProtocolFeatureService;
            this.unconfiguredProject = unconfiguredProject;
            this.appOptionsProvider = appOptionsProvider;
            this.coverageProjectSettingsManager = coverageProjectSettingsManager;
        }

        // visual studio options states that a restart is required.  If this is true then could cache this value
        private async Task<bool> UsingTestingPlatformProtocolAsync()
        {
            var useTestingPlatformProtocolFeature = await useTestingPlatformProtocolFeatureService.GetAsync();
            return useTestingPlatformProtocolFeature == true;
        }

        private bool AllProjectsDisabled()
        {
            var appOptions = appOptionsProvider.Get();
            return !appOptions.Enabled && appOptions.DisabledNoCoverage;
        }

        private async Task<bool> ProjectEnabledAsync()
        {
            var coverageProject = await GetCoverageProjectAsync();
            if (coverageProject != null)
            {
                var projectSettings = await coverageProjectSettingsManager.GetSettingsAsync(coverageProject);
                return projectSettings.Enabled;
            }
            return true;
        }

        private async Task<Guid?> GetProjectGuidAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var hostObject = unconfiguredProject.Services.HostObject;

            var vsHierarchy = (IVsHierarchy)hostObject;
            if (vsHierarchy != null)
            {
                var success = vsHierarchy.GetGuidProperty((uint)VSConstants.VSITEMID.Root, (int)__VSHPROPID.VSHPROPID_ProjectIDGuid, out Guid projectGuid) == VSConstants.S_OK;

                if (success)
                {
                    return projectGuid;
                }
            }
            return null;
        }

        private async Task<CoverageProject> GetCoverageProjectAsync()
        {
            var projectGuid = await GetProjectGuidAsync();
            if (projectGuid.HasValue)
            {
                return new CoverageProject(appOptionsProvider, null, coverageProjectSettingsManager, null)
                {
                    Id = projectGuid.Value,
                    ProjectFile = unconfiguredProject.FullPath
                };
            }
            return null;
        }

        public override async Task<IImmutableDictionary<string, string>> GetGlobalPropertiesAsync(CancellationToken cancellationToken)
        {
            /*
                Note that it only matters for ms code coverage but not going to test for that
                Main thing is that FCC does not turn off if user has Enterprise which does support
                the new feature and has turned off FCC.
            */
            if (await UsingTestingPlatformProtocolAsync() && !AllProjectsDisabled() && await ProjectEnabledAsync())
            {
                // https://github.com/microsoft/testfx/blob/main/src/Platform/Microsoft.Testing.Platform.MSBuild/buildMultiTargeting/Microsoft.Testing.Platform.MSBuild.targets
                return Empty.PropertiesMap.Add("DisableTestingPlatformServerCapability", "true");
            }
            return Empty.PropertiesMap;
        }
    }
}
