using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using FineCodeCoverage.Core.Utilities;
using FineCodeCoverage.Engine.Model;
using FineCodeCoverage.Engine.MsTestPlatform.CodeCoverage;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace FineCodeCoverage.Core.MsTestPlatform.TestingPlatform
{
    [Export(typeof(ITUnitCoverageProjectFactory))]
    internal sealed class TUnitCoverageProjectFactory : ITUnitCoverageProjectFactory
    {
        private readonly ICoverageProjectFactory _coverageProjectFactory;
        private readonly ITemplatedRunSettingsService _templatedRunSettingsService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IXmlUtils _xmlUtils;
        private readonly IRunSettingsToConfiguration _runSettingsToConfiguration;

        private sealed class TUnitCoverageProject : ITUnitCoverageProject
        {
            private readonly Func<CancellationToken, Task<string>> _configurationProvider;

            public TUnitCoverageProject(
                string exePath,
                ICoverageProject coverageProject,
                IVsHierarchy vsHierarchy,
                CommandLineParseResult commandLineParseResult,
                Func<CancellationToken, Task<string>> configurationProvider,
                bool hasCoverageExtension)
            {
                ExePath = exePath;
                CoverageProject = coverageProject;
                VsHierarchy = vsHierarchy;
                CommandLineParseResult = commandLineParseResult;
                _configurationProvider = configurationProvider;
                HasCoverageExtension = hasCoverageExtension;
            }

            public string ExePath { get; }

            public Task<string> GetConfigurationAsync(CancellationToken cancellationToken)
                => _configurationProvider(cancellationToken);

            public ICoverageProject CoverageProject { get; }

            public IVsHierarchy VsHierarchy { get; }

            public CommandLineParseResult CommandLineParseResult { get; }

            public bool HasCoverageExtension { get; }
        }

        [ImportingConstructor]
        public TUnitCoverageProjectFactory(
            ICoverageProjectFactory coverageProjectFactory,
            ITemplatedRunSettingsService templatedRunSettingsService,
            [Import(typeof(SVsServiceProvider))]
            IServiceProvider serviceProvider,
            IXmlUtils xmlUtils,
            IRunSettingsToConfiguration runSettingsToConfiguration)
        {
            _coverageProjectFactory = coverageProjectFactory;
            _templatedRunSettingsService = templatedRunSettingsService;
            _serviceProvider = serviceProvider;
            _xmlUtils = xmlUtils;
            _runSettingsToConfiguration = runSettingsToConfiguration;
        }

        private async Task<ICoverageProject> CreateCoverageProjectAsync(
            IVsHierarchy project,
            CancellationToken cancellationToken)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            ICoverageProject coverageProject = _coverageProjectFactory.Create();
            _ = project.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_Name, out object projectName);
            coverageProject.ProjectName = projectName.ToString();
            _ = project.GetGuidProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_CmdUIGuid, out Guid projectGuid);
            coverageProject.Id = projectGuid;

            // _ = project.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID4.VSHPROPID_TargetFrameworkMoniker, out object targetFrameworkMoniker);
            cancellationToken.ThrowIfCancellationRequested();
            if (project is IVsBuildPropertyStorage buildPropertyStorage)
            {
                // todo configuration parameter for Debug
                int hr = buildPropertyStorage.GetPropertyValue("TargetPath", null, 1, out string outputFile);
                _ = ErrorHandler.ThrowOnFailure(hr);
                coverageProject.TestDllFile = outputFile;
            } // todo throw if not

            cancellationToken.ThrowIfCancellationRequested();
            if (project is IVsProject vsProject)
            {
                int hr = vsProject.GetMkDocument(VSConstants.VSITEMID_ROOT, out string projectFilePath);
                _ = ErrorHandler.ThrowOnFailure(hr);
                coverageProject.ProjectFilePath = projectFilePath;
            } // todo throw if not

            return coverageProject;
        }

        private async Task<string> GetSolutionDirectoryAsync(CancellationToken cancellationToken)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            var vsSolution = _serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
            Assumes.Present(vsSolution);
            _ = vsSolution.GetSolutionInfo(out string solutionDirectory, out _, out string __);
            return solutionDirectory;
        }

        private async Task<XElement> GetConfigurationElementAsync(ICoverageProject coverageProject, CancellationToken ct)
        {
            string solutionDirectory = await GetSolutionDirectoryAsync(ct);
            string runSettings = _templatedRunSettingsService.CreateProjectsRunSettings(
                new ICoverageProject[] { coverageProject },
                solutionDirectory,
                string.Empty)[0].RunSettings;
            return _runSettingsToConfiguration.ConvertToConfiguration(XElement.Parse(runSettings));
        }

        public async Task<ITUnitCoverageProject> CreateTUnitCoverageProjectAsync(
            ITUnitProject tUnitProject,
            CancellationToken cancellationToken)
        {
            ICoverageProject coverageProject = await CreateCoverageProjectAsync(tUnitProject.Hierarchy, cancellationToken);
            string exePath = Path.ChangeExtension(coverageProject.TestDllFile, ".exe");

            return new TUnitCoverageProject(
                exePath,
                coverageProject,
                tUnitProject.Hierarchy,
                tUnitProject.CommandLineParseResult,
                async (ct) =>
                {
                    XElement configurationElement = await GetConfigurationElementAsync(coverageProject, ct);
                    if (coverageProject.Settings.IncludeTestAssembly)
                    {
                        configurationElement.Add(new XElement("IncludeTestAssembly", true));
                    }

                    return _xmlUtils.Serialize(configurationElement);
                },
                tUnitProject.HasCoverageExtension);
        }
    }
}
